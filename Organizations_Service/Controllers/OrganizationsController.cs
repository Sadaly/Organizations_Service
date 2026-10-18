using Microsoft.AspNetCore.Mvc;
using OrganizationsService.Applications.Abstraction;
using OrganizationsService.Domain.Exceptions;
using OrganizationsService.Web.Models;

namespace OrganizationsService.Web.Controllers
{
    public class OrganizationsController(IOrganizationService service) : Controller
    {
        // GET: /Organizations
        public async Task<IActionResult> Index(string? search, CancellationToken ct)
        {
            ViewData["Search"] = search;
            var organizations = await service.GetAllAsync(search, ct);
            return View(organizations);
        }

        // GET: /Organizations/Details/5
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var organization = await service.GetByIdAsync(id, ct);
            if (organization is null)
                return NotFound();
            return View(organization);
        }

        // GET: /Organizations/Create
        public IActionResult Create() => View(new OrganizationFormModel());

        // POST: /Organizations/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganizationFormModel form, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(form);
            try
            {
                await service.CreateAsync(form.ToInput(), ct);
            }
            catch (DuplicateTinException)
            {
                ModelState.AddModelError(nameof(form.Tin), "An organization with this TIN already exists.");
                return View(form);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Organizations/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrganizationFormModel form, CancellationToken ct)
        {
            // The route id and the hidden form field must point to the same organization.
            if (id != form.Id)
                return BadRequest();
            if (!ModelState.IsValid)
                return View(form);
            try
            {
                await service.UpdateAsync(id, form.ToInput(), ct);
            }
            catch (DuplicateTinException)
            {
                ModelState.AddModelError(nameof(form.Tin), "An organization with this TIN already exists.");
                return View(form);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Organizations/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var organization = await service.GetByIdAsync(id, ct);
            if (organization is null)
                return NotFound();
            return View(OrganizationFormModel.FromDto(organization));
        }

        // GET: /Organizations/Delete/5
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var organization = await service.GetByIdAsync(id, ct);
            if (organization is null)
                return NotFound();
            return View(organization);
        }

        // POST: /Organizations/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            try
            {
                await service.DeleteAsync(id, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
