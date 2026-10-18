using Microsoft.AspNetCore.Mvc;
using OrganizationsService.Applications.DTO;
using OrganizationsService.Applications.Service;
using OrganizationsService.Domain.Entities;
using OrganizationsService.Unittests.Fakes;
using OrganizationsService.Web.Controllers;
using OrganizationsService.Web.Models;

namespace OrganizationsService.Unittests;

/// <summary>
/// Unit tests for the MVC controller: which result is returned in each scenario.
/// The real service runs on top of the in-memory repository, so no mocking library is needed.
/// </summary>
public class OrganizationsControllerTests
{
    private readonly InMemoryOrganizationRepository _repo = new InMemoryOrganizationRepository().Seed(
        new Organization { Name = "Alpha", Tin = "1111111111" });

    private OrganizationsController Controller => new(new OrganizationService(_repo));

    private static OrganizationFormModel Form(int id = 0, string name = "Beta", string tin = "2222222222") =>
        new() { Id = id, Name = name, Tin = tin };

    [Fact]
    public async Task Index_PassesListToView()
    {
        var result = Assert.IsType<ViewResult>(await Controller.Index(null, default));

        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<OrganizationDto>>(result.Model));
    }

    [Fact]
    public async Task Details_Unknown_ReturnsNotFound()
    {
        Assert.IsType<NotFoundResult>(await Controller.Details(42, default));
    }

    [Fact]
    public void Create_Get_ReturnsEmptyForm()
    {
        var result = Assert.IsType<ViewResult>(Controller.Create());

        Assert.IsType<OrganizationFormModel>(result.Model);
    }

    [Fact]
    public async Task Create_Valid_RedirectsToIndex()
    {
        var result = await Controller.Create(Form(), default);

        Assert.Equal(nameof(OrganizationsController.Index), Assert.IsType<RedirectToActionResult>(result).ActionName);
        Assert.Equal(2, _repo.Items.Count);
    }

    [Fact]
    public async Task Create_InvalidModelState_ReturnsSameViewWithoutSaving()
    {
        var controller = Controller;
        controller.ModelState.AddModelError(nameof(OrganizationFormModel.Name), "Required");
        var form = Form();

        var result = Assert.IsType<ViewResult>(await controller.Create(form, default));

        Assert.Same(form, result.Model);
        Assert.Single(_repo.Items);
    }

    [Fact]
    public async Task Create_DuplicateTin_AddsModelError()
    {
        var controller = Controller;

        Assert.IsType<ViewResult>(await controller.Create(Form(tin: "1111111111"), default));
        Assert.True(controller.ModelState.ContainsKey(nameof(OrganizationFormModel.Tin)));
    }

    [Fact]
    public async Task Edit_Get_ReturnsFilledFormModel()
    {
        var result = Assert.IsType<ViewResult>(await Controller.Edit(1, default));

        Assert.Equal("Alpha", Assert.IsType<OrganizationFormModel>(result.Model).Name);
    }

    [Fact]
    public async Task Edit_Get_Unknown_ReturnsNotFound()
    {
        Assert.IsType<NotFoundResult>(await Controller.Edit(99, default));
    }

    [Fact]
    public async Task Edit_Valid_UpdatesAndRedirects()
    {
        var result = await Controller.Edit(1, Form(id: 1, name: "Alpha 2", tin: "1111111111"), default);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Alpha 2", _repo.Items.Single().Name);
    }

    [Fact]
    public async Task Edit_IdMismatch_ReturnsBadRequest()
    {
        Assert.IsType<BadRequestResult>(await Controller.Edit(1, Form(id: 2), default));
    }

    [Fact]
    public async Task Edit_Unknown_ReturnsNotFound()
    {
        Assert.IsType<NotFoundResult>(await Controller.Edit(99, Form(id: 99), default));
    }

    [Fact]
    public async Task DeleteConfirmed_Existing_RemovesAndRedirects()
    {
        Assert.IsType<RedirectToActionResult>(await Controller.DeleteConfirmed(1, default));
        Assert.Empty(_repo.Items);
    }

    [Fact]
    public async Task DeleteConfirmed_Unknown_ReturnsNotFound()
    {
        Assert.IsType<NotFoundResult>(await Controller.DeleteConfirmed(99, default));
    }
}
