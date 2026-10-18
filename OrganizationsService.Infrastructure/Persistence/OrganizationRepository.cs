using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrganizationsService.Domain.Entities;
using OrganizationsService.Domain.Exceptions;
using OrganizationsService.Domain.Repositories;

namespace OrganizationsService.Infrastructure.Persistence
{
    internal sealed class OrganizationRepository(AppDbContext db) : IOrganizationRepository
    {
        public async Task<IReadOnlyList<Organization>> ListAsync(string? search, CancellationToken ct = default)
        {
            var query = db.Organizations.AsNoTracking();
            if (search is not null)
            {
                var pattern = $"%{EscapeLike(search)}%";
                query = query.Where(o => EF.Functions.ILike(o.Name, pattern, @"\") || EF.Functions.ILike(o.Tin, pattern, @"\"));
            }
            return await query.OrderBy(o => o.Name).ThenBy(o => o.Id).ToListAsync(ct);
        }

        public async Task<Organization?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task AddAsync(Organization organization, CancellationToken ct = default)
        {
            db.Organizations.Add(organization);
            await SaveAsync(organization, ct);
        }

        public async Task UpdateAsync(Organization organization, CancellationToken ct = default)
        {
            if (db.Entry(organization).State == EntityState.Detached)
                db.Organizations.Update(organization);
            await SaveAsync(organization, ct);
        }

        public async Task<bool> TinInUseAsync(string tin, int? excludeId = null, CancellationToken ct = default) =>
            await db.Organizations.AnyAsync(o => o.Tin == tin && (excludeId == null || o.Id != excludeId), ct);
        public async Task DeleteAsync(Organization organization, CancellationToken ct = default)
        {
            db.Organizations.Remove(organization);
            await db.SaveChangesAsync(ct);
        }

        // Shared by Add and Update: if a concurrent request inserted the same TIN after the service-level check,
        // the unique index fires and we translate it into the domain exception instead of leaking a DbUpdateException.
        private async Task SaveAsync(Organization organization, CancellationToken ct = default)
        {
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when(ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                db.Entry(organization).State = EntityState.Detached;
                throw new DuplicateTinException(organization.Tin);
            }
        }

        private static string EscapeLike(string value) =>
            value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
    }
}