using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop_service.Implementation;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;

    public MenuService(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<List<CategoryViewModel>> GetCategoriesAsync()
    {
        return await _menuRepository.GetCategoriesAsync();
    }

    public async Task<List<ItemViewModel>> GetItemsAsync()
    {
        return await _menuRepository.GetItemsAsync();
    }

    public async Task<Category> AddCategoryAsync(string name, string description, string Createdby)
    {
        var category = new Category { Name = name, Createdby = Createdby, Description = description };
        return await _menuRepository.AddCategoryAsync(category);
    }

    public async Task<CategoryViewModel?> GetCategoryByNameAsync(string name)
    {
        var category = await _menuRepository.GetCategoryByNameAsync(name);
        return category != null ? new CategoryViewModel { Name = category.Name, Id = category.Id } : null;
    }


    public async Task<bool> UpdateCategoryAsync(Category category, string Updatedby)
    {
        return await _menuRepository.UpdateCategoryAsync(category, Updatedby);
    }

    public async Task<CategoryViewModel?> GetCategoryByIdAsync(int id)
    {
        var category = await _menuRepository.GetCategoryByIdAsync(id);
        if (category == null) return null;

        return new CategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }


    public async Task<bool> SoftDeleteCategoryAsync(int id)
    {
        return await _menuRepository.SoftDeleteCategoryAsync(id);
    }

    public async Task<List<ItemViewModel>> GetItemsByCategoryAsync(int categoryId)
    {
        return await _menuRepository.GetItemsByCategoryAsync(categoryId);
    }

    public async Task<PagedResult<ItemViewModel>> GetItemsByCategoryAsync(int categoryId, int pageNumber, int pageSize, string searchTerm = "")
    {
        return await _menuRepository.GetItemsByCategoryAsync(categoryId, pageNumber, pageSize, searchTerm);
    }



    public async Task<bool> AddItemAsync(ItemViewModel model)
    {
        var newItem = new MenuItem
        {
            Name = model.Name,
            Categoryid = model.Categoryid,
            Type = model.ItemType,
            Rate = model.Rate,
            Quantity = model.Quantity,
            UnitType = model.Unit,
            IsAvailable = model.IsAvailable,
            IsdefaultTax = model.IsDefaultTax,
            TaxPercentage = model.TaxPercentage,
            ShortCode = model.ShortCode,
            Description = model.Description,
            ItemImage = model.ItemImagePath
        };

        bool itemAdded = await _menuRepository.AddItemAsync(newItem);

        if (itemAdded && model.ModifierGroups != null && model.ModifierGroups.Any())
        {
            var modifierMappings = model.ModifierGroups.Select(mg => new MappingMenuItemWithModifier
            {
                MenuItemId = newItem.Id,
                ModifierGroupId = mg.GroupId,
                MinModifierCount = mg.MinQuantity,
                MaxModifierCount = mg.MaxQuantity
            }).ToList();

            return await _menuRepository.AddItemModifiersAsync(modifierMappings);
        }

        return itemAdded;
    }

    public async Task<bool> UpdateItemAsync(ItemViewModel model, int id)
    {
        var item = _menuRepository.GetItemsById(id);
        if (item == null) return false;

        var existingModifiers = await _menuRepository.GetItemWithModifiersByItemIdAsync(id);

        var newModifiers = model.ModifierGroups?.Select(mg => new MappingMenuItemWithModifier
        {
            MenuItemId = item.Id,
            ModifierGroupId = mg.GroupId,
            MinModifierCount = mg.MinQuantity,
            MaxModifierCount = mg.MaxQuantity
        }).ToList() ?? new List<MappingMenuItemWithModifier>();

        bool itemChanged =
            item.Name != model.Name ||
            item.Type != model.ItemType ||
            item.Rate != model.Rate ||
            item.Quantity != model.Quantity ||
            item.UnitType != model.Unit ||
            item.IsAvailable != model.IsAvailable ||
            item.IsdefaultTax != model.IsDefaultTax ||
            item.TaxPercentage != model.TaxPercentage ||
            item.ShortCode != model.ShortCode ||
            item.Description != model.Description ||
            item.ItemImage != model.ItemImagePath;

        bool modifiersChanged =
       existingModifiers.Count != newModifiers.Count ||
       existingModifiers.Any(em =>
           !newModifiers.Any(nm =>
               nm.ModifierGroupId == em.ModifierGroupId &&
               nm.MinModifierCount == em.MinModifierCount &&
               nm.MaxModifierCount == em.MaxModifierCount
           )
       ) ||
       newModifiers.Any(nm =>
           !existingModifiers.Any(em =>
               em.ModifierGroupId == nm.ModifierGroupId &&
               em.MinModifierCount == nm.MinModifierCount &&
               em.MaxModifierCount == nm.MaxModifierCount
           )
       );


        if (!itemChanged && !modifiersChanged)
        {
            return false;
        }

        if (itemChanged)
        {
            item.Name = model.Name;
            item.Categoryid = model.Categoryid;
            item.Type = model.ItemType;
            item.Rate = model.Rate;
            item.Quantity = model.Quantity;
            item.UnitType = model.Unit;
            item.IsAvailable = model.IsAvailable;
            item.IsdefaultTax = model.IsDefaultTax;
            item.TaxPercentage = model.TaxPercentage;
            item.ShortCode = model.ShortCode;
            item.Description = model.Description;
            item.ItemImage = model.ItemImagePath;

            bool itemUpdated = await _menuRepository.UpdateItemAsync(item);
            if (!itemUpdated) return false;
        }

        if (modifiersChanged)
        {
            var modifiersToAdd = newModifiers
                .Where(nm => !existingModifiers.Any(em => em.ModifierGroupId == nm.ModifierGroupId))
                .ToList();

            var modifiersToRemove = existingModifiers
                .Where(em => !newModifiers.Any(nm => nm.ModifierGroupId == em.ModifierGroupId))
                .ToList();

            var modifiersToUpdate = existingModifiers
                .Where(em => newModifiers.Any(nm =>
                    nm.ModifierGroupId == em.ModifierGroupId &&
                    (nm.MinModifierCount != em.MinModifierCount || nm.MaxModifierCount != em.MaxModifierCount) 
                ))
                .Select(em => newModifiers.First(nm => nm.ModifierGroupId == em.ModifierGroupId)) 
                .ToList();

            if (modifiersToAdd.Any())
            {
                await _menuRepository.AddItemModifiersAsync(modifiersToAdd);
            }

            if (modifiersToRemove.Any())
            {
                await _menuRepository.DeleteItemModifiersAsync(modifiersToRemove);

            }
            if (modifiersToUpdate.Any())
            {
                await _menuRepository.UpdateItemModifiersAsync(modifiersToUpdate);
            }
        }


        return true;

    }

    public async Task<bool> SoftDeleteItemAsync(int id)
    {
        return await _menuRepository.SoftDeleteItemAsync(id);
    }

    public void SoftDeleteItemsAsync(List<int> itemIds)
    {
        _menuRepository.SoftDeleteItemsAsync(itemIds);
    }

    public ItemViewModel GetItemById(int id)
    {
        return _menuRepository.GetItemById(id);
    }


    public async Task<List<ModifierGroupViewModel>> GetModifierGroupAsync()
    {
        return await _menuRepository.GetModifierGroupAsync();
    }

    public async Task<PagedResult<ModifierViewModel>> GetModifiersByModifierGroupAsync(int modifierGroupId, int pageNumber, int pageSize, string searchTerm = "")
    {
        return await _menuRepository.GetModifiersByModifierGroupAsync(modifierGroupId, pageNumber, pageSize, searchTerm);
    }


    public async Task<List<ModifierViewModel>> GetModifiersAsync()
    {
        return await _menuRepository.GetModifiersAsync();
    }

    public async Task<bool> AddModifierGroup(ModifierGroupViewModel model)
    {
        if (await _menuRepository.ExistsModifierGroupByNameAsync(model.Name))
        {
            return false;
        }

        var modifierGroup = new Modifiergroup
        {
            Name = model.Name,
            Description = model.Description
        };

        return await _menuRepository.AddModifierGroup(modifierGroup, model.ModifierIds);
    }

    public async Task<bool> SoftDeleteModifierGroupAsync(int id)
    {
        return await _menuRepository.SoftDeleteModifierGroupAsync(id);
    }

    public ModifierGroupViewModel GetModifierGroupById(int id)
    {
        return _menuRepository.GetModifierGroupById(id);
    }

    public async Task<bool> UpdateModifierGroup(ModifierGroupViewModel model)
    {
        var existingModifierGroup = await _menuRepository.GetModifierGorupByIdAsync(model.Id);
        if (existingModifierGroup == null)
        {
            return false; // Modifier group not found
        }

        // Check if no changes were made
        if (existingModifierGroup.Name == model.Name
            && existingModifierGroup.Description == model.Description
            && existingModifierGroup.Modifiergroupmodifiers.Select(mgm => mgm.Modifierid).OrderBy(id => id)
                .SequenceEqual(model.ModifierIds.OrderBy(id => id)))
        {
            return false; // No changes detected
        }

        return await _menuRepository.UpdateModifierGroup(model);
    }


    public async Task<bool> AddModifierAsync(ModifierViewModel model)
    {
        return await _menuRepository.AddModifierAsync(model);
    }


    public ModifierViewModel GetModifierById(int id)
    {
        return _menuRepository.GetModifierById(id);
    }



    public async Task<bool> UpdateModifierAsync(ModifierViewModel model)
    {
        var modifier = await _menuRepository.GetModifierByIdAsync(model.Id);
        if (modifier == null) return false;


        bool isModified = modifier.Isdeleted;
        if (modifier.Isdeleted)
        {
            modifier.Isdeleted = false;
            isModified = true;
        }


        if (modifier.Name != model.Name ||
            modifier.Price != model.Price ||
            modifier.Quantity != model.Quantity ||
            modifier.Unittype != model.Unittype ||
            modifier.Description != model.Description)
        {
            modifier.Name = model.Name;
            modifier.Price = model.Price;
            modifier.Quantity = model.Quantity;
            modifier.Unittype = model.Unittype;
            modifier.Description = model.Description;
            isModified = true;
        }


        var existingGroupIds = await _menuRepository.GetModifierGroupIdsByModifierId(model.Id);
        if (!existingGroupIds.OrderBy(x => x).SequenceEqual(model.ModifierGroupIds.OrderBy(x => x)))
        {
            await _menuRepository.UpdateModifierGroupsAsync(model.Id, model.ModifierGroupIds);
            isModified = true;
        }


        if (!isModified)
        {
            return false;
        }

        return await _menuRepository.UpdateModifierAsync(modifier);
    }


    public async Task<bool> SoftDeleteModifierAsync(int id)
    {
        return await _menuRepository.SoftDeleteModifierAsync(id);
    }

    public async Task<bool> SoftDeleteModifierFromGroupAsync(int modifierId, int groupId)
    {
        return await _menuRepository.SoftDeleteModifierFromGroupAsync(modifierId, groupId);
    }

    public async Task<bool> SoftDeleteModifiersAsync(List<int> modifierIds, int currentGroupId)
    {
        return await _menuRepository.SoftDeleteModifiersAsync(modifierIds, currentGroupId);
    }
    public async Task<PagedResult<ModifierViewModel>> GetAllModifiersToAddModifierGroupAsync(int pageNumber, int pageSize, string searchTerm = "")
    {
        return await _menuRepository.GetAllModifiersToAddModifierGroupAsync(pageNumber, pageSize, searchTerm);
    }

    public async Task<ModifierGroupViewModel?> GetModifiersByGroupIdAsync(int modifierGroupId)
    {
        return await _menuRepository.GetModifiersByGroupIdAsync(modifierGroupId);
    }

    public async Task<ItemModifierGroupViewModel> GetModifierGroupByIdForItem(int groupId)
    {
        return await _menuRepository.GetModifierGroupByIdForItem(groupId);
    }
}
