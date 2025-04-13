using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Database;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Implementation;

public class OrderAppRepository : IOrderAppRepository
{
    private readonly PizzaShopDbContext _context;

    public OrderAppRepository(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetCategory()
    {
        return await _context.Categories.Where(c => !c.Isdeleted).ToListAsync();
    }

    public async Task<List<Order>> GetOrdersWithItemsAsync(int? categoryId, string status)
    {
        return await _context.Orders.Include(o => o.OrdersTableMappings).ThenInclude(t => t.Table).ThenInclude(t => t.Section)
                                    .Include(o => o.OrderItemsMappings).ThenInclude(oi => oi.OrderItemModifiers)
                                    .Include(o => o.OrderItemsMappings).ThenInclude(oi => oi.Menuitem).Where(o => o.OrderItemsMappings.Any(oi =>
            (categoryId == null || oi.Menuitem.Categoryid == categoryId) &&
            ((status == "Ready" && oi.Preparedquantity > 0) ||
             (status == "In Progress" && (oi.Quantity - oi.Preparedquantity) > 0)))).ToListAsync();
    }

    public async Task<Order?> GetOrderCardWithIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderItemsMappings)
                .ThenInclude(i => i.OrderItemModifiers)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task UpdatePreparedQuantityAsync(int orderId, int itemId, int preparedQuantity)
    {
        var orderItem = await _context.OrderItemsMappings
            .FirstOrDefaultAsync(x => x.Orderid == orderId && x.Id == itemId);

        if (orderItem != null)
        {
            // Prevent exceeding total quantity
            int totalQuantity = orderItem.Quantity; // Nullable safe
            int currentPreparedQty = orderItem.Preparedquantity ?? 0; // Nullable safe

            int updatedPreparedQty = currentPreparedQty + preparedQuantity;

            // Ensure result doesn't exceed totalQuantity and fits into a byte
            orderItem.Preparedquantity = (byte)Math.Min(updatedPreparedQty, totalQuantity);
            await _context.SaveChangesAsync();
        }
    }

}
