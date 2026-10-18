using OrganizationsService.Applications.DTO;
using OrganizationsService.Applications.Service;
using OrganizationsService.Domain.Entities;
using OrganizationsService.Domain.Exceptions;
using OrganizationsService.Unittests.Fakes;

namespace OrganizationsService.Unittests;

/// <summary>
/// Unit tests for the application service: business rules and input normalization.
/// </summary>
public class OrganizationServiceTests
{
    private readonly InMemoryOrganizationRepository _repo = new InMemoryOrganizationRepository().Seed(
        new Organization { Name = "Beta", Tin = "1111111111" },
        new Organization { Name = "Alpha", Tin = "2222222222" });

    private OrganizationService Service => new(_repo);

    private static OrganizationInput Input(string name = "Gamma", string tin = "3333333333") =>
        new(name, tin, null, null, null, null, new DateOnly(2024, 1, 1));

    [Fact]
    public async Task GetAll_ReturnsDtosSortedByName()
    {
        var result = await Service.GetAllAsync(null);

        Assert.Equal(["Alpha", "Beta"], result.Select(o => o.Name));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("  Alpha ", "Alpha")]
    public async Task GetAll_NormalizesSearch(string? search, string? expected)
    {
        await Service.GetAllAsync(search);

        Assert.Equal(expected, _repo.LastSearch);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsDto()
    {
        var dto = await Service.GetByIdAsync(1);

        Assert.NotNull(dto);
        Assert.Equal(("Beta", "1111111111"), (dto.Name, dto.Tin));
    }

    [Fact]
    public async Task GetById_Unknown_ReturnsNull()
    {
        Assert.Null(await Service.GetByIdAsync(999));
    }

    [Fact]
    public async Task Create_TrimsValuesAndTurnsBlankIntoNull()
    {
        var id = await Service.CreateAsync(new OrganizationInput(
            "  Gamma  ", " 3333333333 ", " Supplier ", "  ", "", " a@b.com ", new DateOnly(2024, 5, 6)));

        var saved = Assert.Single(_repo.Items, o => o.Id == id);
        Assert.Equal("Gamma", saved.Name);
        Assert.Equal("3333333333", saved.Tin);
        Assert.Equal("Supplier", saved.Description);
        Assert.Null(saved.Address);
        Assert.Null(saved.Phone);
        Assert.Equal("a@b.com", saved.Email);
        Assert.Equal(new DateOnly(2024, 5, 6), saved.RegisteredAt);
    }

    [Fact]
    public async Task Create_DuplicateTin_ThrowsAndDoesNotSave()
    {
        var ex = await Assert.ThrowsAsync<DuplicateTinException>(() => Service.CreateAsync(Input(tin: " 1111111111")));

        Assert.Contains("1111111111", ex.Message);
        Assert.Equal(2, _repo.Items.Count);
    }

    [Fact]
    public async Task Update_KeepingOwnTin_Succeeds()
    {
        await Service.UpdateAsync(1, Input("Beta 2", "1111111111"));

        Assert.Equal("Beta 2", _repo.Items.Single(o => o.Id == 1).Name);
    }

    [Fact]
    public async Task Update_TinOfAnotherOrganization_ThrowsAndKeepsData()
    {
        await Assert.ThrowsAsync<DuplicateTinException>(() => Service.UpdateAsync(1, Input("Beta", "2222222222")));

        Assert.Equal("1111111111", _repo.Items.Single(o => o.Id == 1).Tin);
    }

    [Fact]
    public async Task Update_Unknown_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service.UpdateAsync(999, Input()));
    }

    [Fact]
    public async Task Delete_RemovesOrganization()
    {
        await Service.DeleteAsync(1);

        Assert.DoesNotContain(_repo.Items, o => o.Id == 1);
    }

    [Fact]
    public async Task Delete_Unknown_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service.DeleteAsync(999));
    }
}
