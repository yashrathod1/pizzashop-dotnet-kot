using Microsoft.AspNetCore.Mvc;
using pizzashop_service.Interface;

namespace pizzashop.Controllers;

public class TablesController : Controller
{
    private readonly IOrderAppService _orderAppService;

    public TablesController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    public IActionResult Index()
    {   
        ViewBag.ActiveNav = "Tables";
        return View();
    }
}
