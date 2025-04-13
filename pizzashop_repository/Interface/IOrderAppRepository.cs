using pizzashop_repository.Models;

namespace pizzashop_repository.Interface;

public interface IOrderAppRepository
{
    Task<List<Category>> GetCategory();

    Task<List<Order>> GetOrdersWithItemsAsync(int? categoryId, string status);

    Task<Order?> GetOrderCardWithIdAsync(int orderId);

    Task UpdatePreparedQuantityAsync(int orderId, int itemId, int preparedQuantity);
}
