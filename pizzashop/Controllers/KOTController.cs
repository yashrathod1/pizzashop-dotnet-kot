using Microsoft.AspNetCore.Mvc;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop.Controllers;

public class KOTController : Controller
{
    private readonly IOrderAppService _orderAppService;

    public KOTController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveNav = "KOT";
        var kotData = await _orderAppService.GetCategoryAsync();
        return View(kotData);
    }

    public async Task<IActionResult> GetOrderCardByCategory(int? categoryId, string status)
    {
        var kotData = await _orderAppService.GetKOTDataAsync(categoryId,status);

        var filteredOrders = kotData.OrderCard;

   
        return PartialView("_OrderCardsSliderPartial", filteredOrders);
    }


    public async Task<IActionResult> GetOrderCardInModal(int orderId)
    {
        var model = await _orderAppService.GetOrderCardByIdAsync(orderId);
        return PartialView("_KOTOrderItemModalPartial", model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePreparedQuantities([FromBody] UpdatePreparedItemsViewModel model)
    {
        await _orderAppService.UpdatePreparedQuantitiesAsync(model);
        return Ok();
    }



}
