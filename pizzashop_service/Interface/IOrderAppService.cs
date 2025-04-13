using pizzashop_repository.ViewModels;

namespace pizzashop_service.Interface;

public interface IOrderAppService
{
     Task<KOTViewModel> GetCategoryAsync();

     Task<KOTViewModel> GetKOTDataAsync(int? categoryId, string status);

     Task<KOTOrderCardViewModel?> GetOrderCardByIdAsync(int orderId);

      Task UpdatePreparedQuantitiesAsync(UpdatePreparedItemsViewModel model);
}
