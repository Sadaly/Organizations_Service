using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrganizationsService.Domain.Entities;
using OrganizationsService.Domain.Exceptions;
using OrganizationsService.Domain.Repositories;

namespace OrganizationsService.Infrastructure.Persistence
{
    internal sealed class OrganizationRepository(AppDbContext db) : IOrganizationRepository
    {
        public async Task<IReadOnlyList<Organization>> ListAsync(string? search, CancellationToken ct)
        {
            var query = db.Organizations.AsNoTracking();
            if (search is not null)
            {
                var pattern = $"%{EscapeLike(search)}%";
                query = query.Where(o => EF.Functions.ILike(o.Name, pattern, @"\") || EF.Functions.ILike(o.Tin, pattern, @"\"));
            }
            return await query.OrderBy(o => o.Name).ThenBy(o => o.Id).ToListAsync(ct);
        }

        public async Task<Organization?> GetByIdAsync(int id, CancellationToken ct) =>
            await db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task AddAsync(Organization organization, CancellationToken ct)
        {
            db.Organizations.Add(organization);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Organization organization, CancellationToken ct)
        {
            if (db.Entry(organization).State == EntityState.Detached)
                db.Organizations.Update(organization);
            await SaveUpdateAsync(organization, ct);
        }

        public async Task<bool> TinInUseAsync(string inn, CancellationToken ct) =>
            await db.Organizations.AsNoTracking().AnyAsync(o => o.Tin == inn, ct);

        public async Task DeleteAsync(Organization organization, CancellationToken ct)
        {
            db.Organizations.Remove(organization);
            await db.SaveChangesAsync(ct);
        }

        private async Task SaveUpdateAsync(Organization organization, CancellationToken ct)
        {
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when(ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                db.Entry(organization).State = EntityState.Detached;
                throw new DuplicateInnException(organization.Tin);
            }
        }

        private static string EscapeLike(string value) =>
            value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
    }
}