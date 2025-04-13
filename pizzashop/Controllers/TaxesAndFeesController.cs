using Microsoft.AspNetCore.Mvc;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop.Controllers;

public class TaxesAndFeesController : Controller
{
    private readonly ITaxesAndFeesService _taxesAndFeesService;

    public TaxesAndFeesController(ITaxesAndFeesService taxesAndFeesService)
    {
        _taxesAndFeesService = taxesAndFeesService;
    }
    public async Task<IActionResult> Index()
    {    
        ViewBag.ActiveNav = "TaxsAndFees";
        var permission = await PermissionHelper.GetPermissionsAsync(HttpContext, "TaxAndFee");
        ViewBag.Permissions = permission;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetTaxesAndFees(int pageNumber = 1, int pageSize = 5, string searchTerm = "")
    {   
        var permission = await PermissionHelper.GetPermissionsAsync(HttpContext, "TaxAndFee");
        ViewBag.Permissions = permission;
        var pagedItems = await _taxesAndFeesService.GetTaxesAndFeesAsync(pageNumber, pageSize, searchTerm);
        return PartialView("_TaxesAndFeesPartial", pagedItems);
    }

    [HttpPost]
    public async Task<IActionResult> AddTax(TaxsAndFeesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data provided." });
        }



        var result = await _taxesAndFeesService.AddTaxAsync(model);

        if (result)
        {
            return Json(new { success = true, message = "Tax added successfully." });
        }
        else
        {
            return Json(new { success = false, message = "Failed to add Tax." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTaxById(int id)
    {
        var tax = await _taxesAndFeesService.GetTaxByIdAsync(id);

        if (tax == null)
        {
            return NotFound();
        }

        return Json(tax);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTax([FromBody] ItemViewModel request)
    {
        bool result = await _taxesAndFeesService.SoftDeleteTaxAsync(request.Id);
        return result ? Ok(new { success = true }) : Json(new { success = false });
    }

    [HttpPost]
    public async Task<IActionResult> EditTax([FromBody] TaxsAndFeesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new
            {
                success = false,
                message = "Invalid data provided.",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        }

        var result = await _taxesAndFeesService.UpdateTaxAsync(model);

        if (!result)
        {
            return Json(new { success = false, message = "An error occurred while updating the table." });
        }

        return Json(new { success = true, message = "Tax updated successfully!" });
    }
}
