using Microsoft.EntityFrameworkCore;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.DTOs.ListDTOs;
using ShoppingList_WebAPI.DTOs.ListItemDTOs;
using ShoppingList_WebAPI.Models;

namespace ShoppingList_WebAPI.Services;

public class ShoppingListService(AppDbContext context) : IShoppingListService
{
    public async Task<IReadOnlyList<ShoppingListResponse>> GetAllListsAsync(int userId, CancellationToken ct)
    {
        var allLists = await context.ShoppingLists
            .Where(x => x.OwnerId == userId || x.SharedWith.Any(s => s.UserId == userId ))
            .Select(x => new ShoppingListResponse
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                OwnerId = x.OwnerId,
                OwnerName =  x.Owner.Name,
                OwnerEmail = x.Owner.Email,
                Items = x.Items.Select(i => new ListItemResponse
                {
                    Id = i.Id,
                    Name = i.Name,
                    Bought = i.Bought,
                    CreatedAt = i.CreatedAt,
                    CreatedByUser = new UserSummary { Id = i.CreatedByUser.Id, Name = i.CreatedByUser.Name },
                    BoughtByUser = i.BoughtByUser != null
                        ? new UserSummary { Id = i.BoughtByUser.Id, Name = i.BoughtByUser.Name }
                        : null,
                    BoughtAt = i.BoughtAt,
                    UpdatedAt = i.UpdatedAt,
                    ListId = i.ListId
                }).ToList()
            })
            .ToListAsync(ct);

        return allLists;
    }

    public async Task<ShoppingListResponse> GetListAsync(int userId, int listId, CancellationToken ct)
    {
        var list = await context.ShoppingLists
            .Where(x => x.OwnerId == userId && x.Id == listId)
            .Select(x => new ShoppingListResponse
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                OwnerId = x.OwnerId,
                OwnerName =  x.Owner.Name,
                OwnerEmail = x.Owner.Email,
                Items = x.Items.Select(i => new ListItemResponse
                {
                    Id = i.Id,
                    Name = i.Name,
                    Bought = i.Bought,
                    CreatedAt = i.CreatedAt,
                    CreatedByUser = new UserSummary { Id = i.CreatedByUser.Id, Name = i.CreatedByUser.Name },
                    UpdatedAt = i.UpdatedAt,
                    BoughtAt = i.BoughtAt,
                    BoughtByUser = i.BoughtByUser != null
                        ? new UserSummary { Id = i.BoughtByUser.Id, Name = i.BoughtByUser.Name }
                        : null,
                    ListId = i.ListId
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
        
        if (list == null)
            throw new KeyNotFoundException("List not found");

        return list;
    }

    public async Task<ShoppingListResponse> CreateListAsync(int userId, CreateShoppingListRequest req, CancellationToken ct)
    {
        var owner = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (owner == null)
            throw new KeyNotFoundException("User not found");

        var newList = new ShoppingList
        {
            Name = req.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OwnerId = userId
        };

        context.ShoppingLists.Add(newList);
        await context.SaveChangesAsync(ct);

        return new ShoppingListResponse
        {
            Id = newList.Id,
            Name = newList.Name,
            CreatedAt = newList.CreatedAt,
            UpdatedAt = newList.UpdatedAt,
            OwnerId = newList.OwnerId,
            OwnerName = owner.Name,
            OwnerEmail = owner.Email,
            Items = new List<ListItemResponse>()
        };
    }

    public async Task<ShoppingListResponse> UpdateListAsync(int userId, int listId, UpdateShoppingListRequest req, CancellationToken ct)
    {
        var list = await context.ShoppingLists
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.Id == listId && x.OwnerId == userId, ct);
        
        if (list == null)
            throw new KeyNotFoundException("List not found");
        
        list.Name = req.Name;
        list.UpdatedAt = DateTime.UtcNow;
        
        await  context.SaveChangesAsync(ct);

        return new ShoppingListResponse
        {
            Id = list.Id,
            Name = list.Name,
            CreatedAt = list.CreatedAt,
            UpdatedAt = list.UpdatedAt,
            OwnerId = list.OwnerId,
            OwnerName = list.Owner.Name,
            OwnerEmail = list.Owner.Email,
            Items = new List<ListItemResponse>()
        };
    }

    public async Task DeleteListAsync(int userId, int listId, CancellationToken ct)
    {
        var list = await context.ShoppingLists
            .FirstOrDefaultAsync(x => x.Id == listId && x.OwnerId == userId, ct);

        if (list != null)
        {
            context.ShoppingLists.Remove(list);
            await context.SaveChangesAsync(ct);
            return;
        }

        var sharedList = await context.SharedLists
            .FirstOrDefaultAsync(x => x.ListId == listId && x.UserId == userId, ct);

        if (sharedList == null)
            throw new KeyNotFoundException("List not found");

        context.SharedLists.Remove(sharedList);
        await context.SaveChangesAsync(ct);
    }
}