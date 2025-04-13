using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Interface;

public interface IMenuRepository
{
    Task<List<CategoryViewModel>> GetCategoriesAsync();

    Task<List<ItemViewModel>> GetItemsAsync();
    Task<Category> AddCategoryAsync(Category category);

    Task<Category?> GetCategoryByNameAsync(string name);

    Task<bool> UpdateCategoryAsync(Category category, string Updatedby);

    Task<Category?> GetCategoryByIdAsync(int id);

    Task<bool> SoftDeleteCategoryAsync(int id);

    Task<List<ItemViewModel>> GetItemsByCategoryAsync(int categoryId);

    Task<bool> AddItemAsync(MenuItem item);

    Task<bool> AddItemModifiersAsync(List<MappingMenuItemWithModifier> mappings);

    Task<bool> UpdateItemAsync(MenuItem item);

    Task<bool> DeleteItemModifiersAsync(List<MappingMenuItemWithModifier> modifiersToRemove);

    MenuItem? GetItemsById(int id);

    Task<bool> SoftDeleteItemAsync(int id);

    void SoftDeleteItemsAsync(List<int> itemIds);

    ItemViewModel GetItemById(int id);

    Task<List<ModifierGroupViewModel>> GetModifierGroupAsync();

    Task<PagedResult<ModifierViewModel>> GetModifiersByModifierGroupAsync(int modifierGroupId, int pageNumber, int pageSize, string searchTerm = " ");

    Task<List<ModifierViewModel>> GetModifiersAsync();

    Task<bool> AddModifierGroup(Modifiergroup modifierGroup, List<int> modifierIds);

    Task<bool> ExistsModifierGroupByNameAsync(string name);

    Task<bool> SoftDeleteModifierGroupAsync(int id);

    ModifierGroupViewModel GetModifierGroupById(int id);

    Task<bool> UpdateModifierGroup(ModifierGroupViewModel model);

    Task<Modifiergroup?> GetModifierGorupByIdAsync(int id);

    Task<bool> AddModifierAsync(ModifierViewModel model);

    ModifierViewModel GetModifierById(int id);

    Task<bool> UpdateModifierAsync(Modifier modifie);

    Task UpdateModifierGroupsAsync(int modifierId, List<int> modifierGroupIds);

    Task<Modifier?> GetModifierByIdAsync(int id);

    Task<bool> SoftDeleteModifierAsync(int id);

    Task<bool> SoftDeleteModifierFromGroupAsync(int modifierId, int groupId);

    Task<bool> SoftDeleteModifiersAsync(List<int> modifierIds, int currentGroupId);

    Task<PagedResult<ItemViewModel>> GetItemsByCategoryAsync(int categoryId, int pageNumber, int pageSize, string searchTerm = "");

    Task<PagedResult<ModifierViewModel>> GetAllModifiersToAddModifierGroupAsync(int pageNumber, int pageSize, string searchTerm = "");

    Task<ModifierGroupViewModel?> GetModifiersByGroupIdAsync(int modifierGroupId);

    Task<List<int>> GetModifierGroupIdsByModifierId(int modifierId);

    Task<ItemModifierGroupViewModel> GetModifierGroupByIdForItem(int groupId);

    Task<List<MappingMenuItemWithModifier>> GetItemWithModifiersByItemIdAsync(int id);

    Task<bool> UpdateItemModifiersAsync(List<MappingMenuItemWithModifier> modifiersToUpdate);
}
