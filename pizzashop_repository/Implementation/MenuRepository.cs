using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Database;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Implementation;

public class MenuRepository : IMenuRepository
{
    private readonly PizzaShopDbContext _context;

    public MenuRepository(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryViewModel>> GetCategoriesAsync()
    {
        return await _context.Categories.Where(c => !c.Isdeleted).Select(x => new CategoryViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        }).ToListAsync();
    }

    public async Task<List<ItemViewModel>> GetItemsAsync()
    {
        return await _context.MenuItems.Where(i => !i.IsDeleted).Select(i => new ItemViewModel
        {
            Id = i.Id,
            Name = i.Name,
            Rate = i.Rate,
            Quantity = i.Quantity,
            IsAvailable = i.IsAvailable,
            ItemType = i.Type,
            ItemImagePath = i.ItemImage,
            Description = i.Description,
            ShortCode = i.ShortCode,
            IsDefaultTax = i.IsdefaultTax,
            Categoryid = i.Categoryid,
            Unit = i.UnitType,
            TaxPercentage = i.TaxPercentage,
        }).ToListAsync();
    }


    public async Task<Category> AddCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        return await _context.Categories.Where(c => !c.Isdeleted).FirstOrDefaultAsync(c => c.Name == name);
    }


    public async Task<bool> UpdateCategoryAsync(Category category, string Updatedby)
    {
        var existingCategory = await _context.Categories.FindAsync(category.Id);
        if (existingCategory == null) return false;

        existingCategory.Name = category.Name;
        existingCategory.Description = category.Description;
        existingCategory.Updatedby = Updatedby;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> SoftDeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        category.Isdeleted = true;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<ItemViewModel>> GetItemsByCategoryAsync(int categoryId)
    {
        var items = await _context.MenuItems
            .Where(i => i.Categoryid == categoryId && !i.IsDeleted)
            .Select(i => new ItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Rate = i.Rate,
                Quantity = i.Quantity,
                IsAvailable = i.IsAvailable,
                ItemType = i.Type,
                ItemImagePath = i.ItemImage,
                Description = i.Description,
                ShortCode = i.ShortCode,
                IsDefaultTax = i.IsdefaultTax,
                Categoryid = i.Categoryid,
                Unit = i.UnitType,
                TaxPercentage = i.TaxPercentage,

            }).ToListAsync();

        return items;
    }

    public async Task<PagedResult<ItemViewModel>> GetItemsByCategoryAsync(int categoryId, int pageNumber, int pageSize, string searchTerm = "")
    {
        var query = _context.MenuItems
            .Where(i => i.Categoryid == categoryId && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i => i.Name.ToLower().Contains(searchTerm.ToLower()));
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(i => i.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Rate = i.Rate,
                Quantity = i.Quantity,
                IsAvailable = i.IsAvailable,
                ItemType = i.Type,
                ItemImagePath = i.ItemImage,
                Description = i.Description,
                ShortCode = i.ShortCode,
                IsDefaultTax = i.IsdefaultTax,
                Categoryid = i.Categoryid,
                Unit = i.UnitType,
                TaxPercentage = i.TaxPercentage
            }).ToListAsync();

        return new PagedResult<ItemViewModel>(items, pageNumber, pageSize, totalCount);
    }



    public async Task<bool> AddItemAsync(MenuItem item)
    {
        _context.MenuItems.Add(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> AddItemModifiersAsync(List<MappingMenuItemWithModifier> mappings)
    {
        _context.MappingMenuItemWithModifiers.AddRange(mappings);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateItemAsync(MenuItem item)
    {
        _context.MenuItems.Update(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteItemModifiersAsync(List<MappingMenuItemWithModifier> modifiersToRemove)
    {
        if (modifiersToRemove == null || !modifiersToRemove.Any())
            return false; // No modifiers to remove

        _context.MappingMenuItemWithModifiers.RemoveRange(modifiersToRemove);
        return await _context.SaveChangesAsync() > 0;
    }



    public MenuItem? GetItemsById(int id)
    {
        return _context.MenuItems.FirstOrDefault(x => x.Id == id);

    }

    public async Task<bool> SoftDeleteItemAsync(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return false;

        item.IsDeleted = true;
        return await _context.SaveChangesAsync() > 0;
    }


    public void SoftDeleteItemsAsync(List<int> itemIds)
    {
        var items = _context.MenuItems.Where(x => itemIds.Contains(x.Id)).ToList();
        foreach (var item in items)
        {
            item.IsDeleted = true;
        }
        _context.SaveChanges();
    }

    public ItemViewModel GetItemById(int id)
    {
        var assignedModifierGroups = _context.MappingMenuItemWithModifiers
        .Where(m => m.MenuItemId == id)
        .Select(m => m.ModifierGroupId)
        .ToList();

        var modifierGroups = _context.MappingMenuItemWithModifiers
                             .Where(m => m.MenuItemId == id)
                             .Join(_context.Modifiergroups,
                              mapping => mapping.ModifierGroupId,
                             group => group.Id,
                         (mapping, group) => new ItemModifierGroupViewModel
                         {
                             Id = group.Id,
                             Name = group.Name,
                             MinQuantity = mapping.MinModifierCount,
                             MaxQuantity = mapping.MaxModifierCount,
                             AvailableModifiersForItem = _context.Modifiergroupmodifiers
                  .Where(mgm => mgm.Modifiergroupid == group.Id && !mgm.Isdeleted && !mgm.Modifier.Isdeleted)
                  .Join(_context.Modifiers,
                        mgm => mgm.Modifierid,
                        mod => mod.Id,
                        (mgm, mod) => new ModifierViewModel
                        {
                            Id = mod.Id,
                            Name = mod.Name,
                            Price = mod.Price
                        })
                        .ToList()
                         }).ToList();



        var item = _context.MenuItems
            .Where(x => x.Id == id)
            .Select(x => new ItemViewModel
            {
                Id = x.Id,
                Categoryid = x.Categoryid,
                Name = x.Name,
                Rate = x.Rate,
                Unit = x.UnitType,
                Quantity = x.Quantity,
                ItemType = x.Type,
                IsAvailable = x.IsAvailable,
                ShortCode = x.ShortCode,
                Description = x.Description,
                TaxPercentage = x.TaxPercentage,
                IsDefaultTax = x.IsdefaultTax == true ? true : false,
                ItemImagePath = x.ItemImage,
                AssignedModifierGroups = assignedModifierGroups,
                ModifierGroups = modifierGroups


            }).FirstOrDefault();

        return item;
    }


    public async Task<List<ModifierGroupViewModel>> GetModifierGroupAsync()
    {
        return await _context.Modifiergroups.Where(c => !c.Isdeleted).Select(x => new ModifierGroupViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        }).ToListAsync();
    }

    public async Task<PagedResult<ModifierViewModel>> GetModifiersByModifierGroupAsync(
    int modifierGroupId, int pageNumber, int pageSize, string searchTerm = "")
    {
        var query = _context.Modifiergroupmodifiers
            .Where(mgm => mgm.Modifiergroupid == modifierGroupId && !mgm.Modifier.Isdeleted && !mgm.Isdeleted)
            .Select(mgm => mgm.Modifier);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i => i.Name.ToLower().Contains(searchTerm.ToLower()));
        }

        int totalCount = await query.CountAsync();

        var modifiers = await query
            .OrderBy(m => m.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ModifierViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Price = m.Price,
                Quantity = m.Quantity,
                Unittype = m.Unittype,
                Description = m.Description,
                Isdeleted = m.Isdeleted
            }).ToListAsync();

        return new PagedResult<ModifierViewModel>(modifiers, pageNumber, pageSize, totalCount);
    }



    public async Task<List<ModifierViewModel>> GetModifiersAsync()
    {
        return await _context.Modifiers
            .Where(m => !m.Isdeleted)
            .Select(m => new ModifierViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Price = m.Price,
                Quantity = m.Quantity,
                Unittype = m.Unittype,
                Description = m.Description,
                ModifierGroupIds = _context.Modifiergroupmodifiers
                    .Where(mgm => mgm.Modifierid == m.Id)
                    .Select(mgm => mgm.Modifiergroupid)
                    .ToList(),
                Isdeleted = m.Isdeleted
            })
            .ToListAsync();
    }


    public async Task<bool> AddModifierGroup(Modifiergroup modifierGroup, List<int> modifierIds)
    {


        await _context.Modifiergroups.AddAsync(modifierGroup);
        await _context.SaveChangesAsync();

        if (modifierIds != null && modifierIds.Count > 0)
        {
            foreach (var modifierId in modifierIds)
            {
                var modifierGroupModifier = new Modifiergroupmodifier
                {
                    Modifiergroupid = modifierGroup.Id,
                    Modifierid = modifierId
                };
                await _context.Modifiergroupmodifiers.AddAsync(modifierGroupModifier);
            }
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> ExistsModifierGroupByNameAsync(string name)
    {
        return await _context.Modifiergroups.AnyAsync(mg => mg.Name.ToLower() == name.ToLower());
    }



    public async Task<bool> SoftDeleteModifierGroupAsync(int id)
    {
        var modifierGroup = await _context.Modifiergroups.FindAsync(id);
        var modifierGroupModifier = await _context.Modifiergroupmodifiers.Where(mgm => mgm.Modifiergroupid == id).ToListAsync();

        if (modifierGroup == null) return false;

        foreach (var modifiergroup in modifierGroupModifier)
        {
            modifiergroup.Isdeleted = true;
        }
        modifierGroup.Isdeleted = true;
        return await _context.SaveChangesAsync() > 0;
    }

    public ModifierGroupViewModel GetModifierGroupById(int id)
    {
        var modifierGroup = _context.Modifiergroups
            .Include(mg => mg.Modifiergroupmodifiers)
            .ThenInclude(mgm => mgm.Modifier)
            .FirstOrDefault(mg => mg.Id == id);

        if (modifierGroup == null) return null;

        return new ModifierGroupViewModel
        {
            Id = modifierGroup.Id,
            Name = modifierGroup.Name,
            Description = modifierGroup.Description,
            AvailableModifiers = modifierGroup.Modifiergroupmodifiers
            .Where(mgm => !mgm.Modifier.Isdeleted && !mgm.Isdeleted)
            .Select(mgm => new ModifierViewModel
            {
                Id = mgm.Modifier.Id,
                Name = mgm.Modifier.Name
            }).ToList(),

            // Store IDs of selected modifiers
            ModifierIds = modifierGroup.Modifiergroupmodifiers
            .Where(mgm => !mgm.Modifier.Isdeleted && !mgm.Isdeleted)
            .Select(mgm => mgm.Modifierid)
            .ToList()
        };
    }


    public async Task<bool> UpdateModifierGroup(ModifierGroupViewModel model)
    {
        var existingGroup = await _context.Modifiergroups
            .Include(mg => mg.Modifiergroupmodifiers)
            .FirstOrDefaultAsync(mg => mg.Id == model.Id);

        if (existingGroup != null)
        {
            existingGroup.Name = model.Name;
            existingGroup.Description = model.Description;

            var existingModifierIds = existingGroup.Modifiergroupmodifiers
           .Select(m => m.Modifierid)
           .ToList();


            var removeModifierIds = existingModifierIds.Except(model.ModifierIds).ToList();
            var newModifierIds = model.ModifierIds.Except(existingModifierIds).ToList();
            foreach (var modifierId in newModifierIds)
            {
                existingGroup.Modifiergroupmodifiers.Add(new Modifiergroupmodifier
                {
                    Modifiergroupid = model.Id,
                    Modifierid = modifierId
                });
            }

            foreach (var modifier in existingGroup.Modifiergroupmodifiers.Where(m => removeModifierIds.Contains(m.Modifierid)))
            {
                modifier.Isdeleted = true;
            }

            return await _context.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<Modifiergroup?> GetModifierGorupByIdAsync(int id)
    {
        return await _context.Modifiergroups.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> AddModifierAsync(ModifierViewModel model)
    {
        var modifier = new Modifier
        {
            Name = model.Name,
            Price = model.Price,
            Unittype = model.Unittype,
            Quantity = model.Quantity,
            Description = model.Description,
            Isdeleted = false
        };

        _context.Modifiers.Add(modifier);
        await _context.SaveChangesAsync();


        if (model.ModifierGroupIds != null && model.ModifierGroupIds.Any())
        {
            var mappings = model.ModifierGroupIds.Select(groupId => new Modifiergroupmodifier
            {
                Modifierid = modifier.Id,
                Modifiergroupid = groupId
            });

            await _context.Modifiergroupmodifiers.AddRangeAsync(mappings);
        }

        return await _context.SaveChangesAsync() > 0;
    }



    public ModifierViewModel GetModifierById(int id)
    {
        var modifier = _context.Modifiers
            .Where(x => x.Id == id)
            .Select(x => new ModifierViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                Unittype = x.Unittype,
                Quantity = x.Quantity,
                Description = x.Description,
                Isdeleted = x.Isdeleted,
                ModifierGroupIds = _context.Modifiergroupmodifiers
                    .Where(mgm => mgm.Modifierid == x.Id && !mgm.Isdeleted)
                    .Select(mgm => mgm.Modifiergroupid)
                    .ToList()
            })
            .FirstOrDefault();

        return modifier;
    }


    public async Task<bool> UpdateModifierAsync(Modifier modifie)
    {
        _context.Modifiers.Update(modifie);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task UpdateModifierGroupsAsync(int modifierId, List<int> modifierGroupIds)
    {

        var existingMappings = await _context.Modifiergroupmodifiers
            .Where(mgm => mgm.Modifierid == modifierId)
            .ToListAsync();

        var activeGroupIds = existingMappings
            .Where(mgm => !mgm.Isdeleted)
            .Select(mgm => mgm.Modifiergroupid)
            .ToList();

        var groupsToRemove = activeGroupIds.Except(modifierGroupIds).ToList();
        var groupsToAdd = modifierGroupIds.Except(activeGroupIds).ToList();


        foreach (var mapping in existingMappings.Where(mgm => groupsToRemove.Contains(mgm.Modifiergroupid)))
        {
            mapping.Isdeleted = true;
        }

        foreach (var mapping in existingMappings.Where(mgm => groupsToAdd.Contains(mgm.Modifiergroupid)))
        {
            mapping.Isdeleted = false;
            groupsToAdd.Remove(mapping.Modifiergroupid);
        }


        if (groupsToAdd.Any())
        {
            var newMappings = groupsToAdd.Select(groupId => new Modifiergroupmodifier
            {
                Modifierid = modifierId,
                Modifiergroupid = groupId,
                Isdeleted = false
            }).ToList();

            await _context.Modifiergroupmodifiers.AddRangeAsync(newMappings);
        }

        await _context.SaveChangesAsync();
    }


    public async Task<Modifier?> GetModifierByIdAsync(int id)
    {
        return await _context.Modifiers
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> SoftDeleteModifierAsync(int id)
    {

        var modifier = await _context.Modifiers.FindAsync(id);
        if (modifier == null) return false;

        modifier.Isdeleted = true;

        var relatedModifierGroups = _context.Modifiergroupmodifiers.Where(mgm => mgm.Modifierid == id).ToList();

        foreach (var mgm in relatedModifierGroups)
        {
            mgm.Isdeleted = true;
        }
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteModifierFromGroupAsync(int modifierId, int groupId)
    {
        var mapping = await _context.Modifiergroupmodifiers
            .FirstOrDefaultAsync(mgm => mgm.Modifierid == modifierId && mgm.Modifiergroupid == groupId);

        if (mapping != null)
        {
            mapping.Isdeleted = true; // Soft delete only in the selected group
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> SoftDeleteModifiersAsync(List<int> modifierIds, int currentGroupId)
    {
        if (modifierIds == null || !modifierIds.Any())
            return false;

        var modifiersToUpdate = await _context.Modifiergroupmodifiers
            .Where(mgm => modifierIds.Contains(mgm.Modifierid) && mgm.Modifiergroupid == currentGroupId)
            .ToListAsync();

        if (!modifiersToUpdate.Any())
            return false;

        foreach (var modifier in modifiersToUpdate)
        {
            modifier.Isdeleted = true;
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<PagedResult<ModifierViewModel>> GetAllModifiersToAddModifierGroupAsync(int pageNumber, int pageSize, string searchTerm = "")
    {
        var query = _context.Modifiers
            .Where(m => !m.Isdeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(m => m.Name.Contains(searchTerm));
        }

        int totalCount = await query.CountAsync();

        var modifiers = await query
            .OrderBy(m => m.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ModifierViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Price = m.Price,
                Quantity = m.Quantity,
                Unittype = m.Unittype,
                Description = m.Description
            }).ToListAsync();

        return new PagedResult<ModifierViewModel>(modifiers, pageNumber, pageSize, totalCount);
    }

    public async Task<ModifierGroupViewModel?> GetModifiersByGroupIdAsync(int modifierGroupId)
    {
        return await _context.Modifiergroups
            .Where(mg => mg.Id == modifierGroupId)
            .Select(mg => new ModifierGroupViewModel
            {
                Id = mg.Id,
                Name = mg.Name,
                AvailableModifiers = _context.Modifiergroupmodifiers
                    .Where(mgm => mgm.Modifiergroupid == modifierGroupId && !mgm.Isdeleted)
                    .Select(mgm => new ModifierViewModel
                    {
                        Id = mgm.Modifier.Id,
                        Name = mgm.Modifier.Name,
                        Price = mgm.Modifier.Price
                    }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<int>> GetModifierGroupIdsByModifierId(int modifierId)
    {
        return await _context.Modifiergroupmodifiers
            .Where(mgm => mgm.Modifierid == modifierId && !mgm.Isdeleted)
            .Select(mgm => mgm.Modifiergroupid)
            .ToListAsync();
    }


    public async Task<ItemModifierGroupViewModel> GetModifierGroupByIdForItem(int groupId)
    {
        var modifierGroup = await _context.Modifiergroups
            .Include(mg => mg.Modifiergroupmodifiers)
            .ThenInclude(mgm => mgm.Modifier)
            .FirstOrDefaultAsync(mg => mg.Id == groupId);

        if (modifierGroup == null) return null;

        return new ItemModifierGroupViewModel
        {
            Id = modifierGroup.Id,
            Name = modifierGroup.Name,
            AvailableModifiersForItem = modifierGroup.Modifiergroupmodifiers.Where(mgm => !mgm.Isdeleted && !mgm.Modifier.Isdeleted).Select(mgm => new ModifierViewModel
            {
                Id = mgm.Modifier.Id,
                Name = mgm.Modifier.Name,
                Price = mgm.Modifier.Price
            }).ToList()
        };
    }

    public async Task<List<MappingMenuItemWithModifier>> GetItemWithModifiersByItemIdAsync(int id)
    {
        return await _context.MappingMenuItemWithModifiers.Where(m => m.MenuItemId == id).ToListAsync();
    }
    public async Task<bool> UpdateItemModifiersAsync(List<MappingMenuItemWithModifier> modifiersToUpdate)
    {
        foreach (var modifier in modifiersToUpdate)
        {
            var existingModifier = await _context.MappingMenuItemWithModifiers
                .FirstOrDefaultAsync(m => m.MenuItemId == modifier.MenuItemId && m.ModifierGroupId == modifier.ModifierGroupId);

            if (existingModifier != null)
            {
                existingModifier.MinModifierCount = modifier.MinModifierCount;
                existingModifier.MaxModifierCount = modifier.MaxModifierCount;
            }
        }

        return await _context.SaveChangesAsync() > 0;
    }
}







