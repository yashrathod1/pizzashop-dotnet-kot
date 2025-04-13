using pizzashop_repository.Interface;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop_service.Implementation;

public class OrderAppService : IOrderAppService
{
    private readonly IOrderAppRepository _orderAppRepository;

    public OrderAppService(IOrderAppRepository orderAppRepository)
    {
        _orderAppRepository = orderAppRepository;
    }

    public async Task<KOTViewModel> GetCategoryAsync()
    {
        var category = await _orderAppRepository.GetCategory();

        var viewmodel = new KOTViewModel
        {
            KOTCategory = category.Select(c => new KOTCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToList()
        };

        return viewmodel;
    }

    public async Task<KOTViewModel> GetKOTDataAsync(int? categoryId, string status)
    {
        var orders = await _orderAppRepository.GetOrdersWithItemsAsync(categoryId, status);
        var categories = await _orderAppRepository.GetCategory();

        var orderCards = orders.Select(o => new KOTOrderCardViewModel
        {
            OrderId = o.Id,
            CreatedAt = o.Createdat,
            OrderInstruction = o.Comment,
            ItemInstruction = o.OrderItemsMappings.Select(oi => oi.Instruction).FirstOrDefault(),
            Categoryid = o.OrderItemsMappings
              .Select(oi => oi.Menuitem.Categoryid)
              .FirstOrDefault(),

            SectionTable = o.OrdersTableMappings.Select(map => new KOTOrderSectionTableViewModel
            {
                TableName = map.Table?.Name ?? "N/A",
                SectionName = map.Table?.Section?.Name ?? "N/A"
            }).ToList(),
            Items = o.OrderItemsMappings
.Where(item => status == "Ready" || (item.Quantity - item.Preparedquantity) > 0).Select(item => new KOTOrderItemViewModel
{
    ItemName = item.ItemName ?? "Unknown",
    Quantity = status == "Ready"
            ? item.Preparedquantity
            : (item.Quantity - item.Preparedquantity),
    Modifiers = item.OrderItemModifiers
                    .Select(mod => mod.ModifierName ?? "")
                    .ToList()
}).ToList()

        }).ToList();

        var kotCategories = categories.Select(c => new KOTCategoryViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();

        return new KOTViewModel
        {
            KOTCategory = kotCategories,
            OrderCard = orderCards

        };
    }

    public async Task<KOTOrderCardViewModel?> GetOrderCardByIdAsync(int orderId)
    {
        var order = await _orderAppRepository.GetOrderCardWithIdAsync(orderId);

        return new KOTOrderCardViewModel
        {
            OrderId = order.Id,
            Items = order.OrderItemsMappings.Where(item => (item.Quantity - item.Preparedquantity) > 0).Select(i => new KOTOrderItemViewModel
            {
                ItemId = i.Id,
                ItemName = i.ItemName,
                Quantity = i.Quantity - i.Preparedquantity,
                Modifiers = i.OrderItemModifiers.Select(m => m.ModifierName).ToList()
            }).ToList(),

        };
    }

    public async Task UpdatePreparedQuantitiesAsync(UpdatePreparedItemsViewModel model)
    {
        foreach (var item in model.Items)
        {
            await _orderAppRepository.UpdatePreparedQuantityAsync(model.OrderId, item.ItemId, item.PreparedQuantity);
        }
    }

}
