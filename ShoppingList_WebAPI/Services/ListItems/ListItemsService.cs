using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.DTOs.ListItemDTOs;
using ShoppingList_WebAPI.Hubs;
using ShoppingList_WebAPI.Models;

namespace ShoppingList_WebAPI.Services.ListItems;

public class ListItemsService(AppDbContext context, IHubContext<ShoppingListHub> hubContext) : IListItemsService
{
    public async Task<ListItemResponse> AddListItemAsync(int userId, string userName, int listId, AddListItemRequest req, CancellationToken ct)
    {
        var existingList = await context.ShoppingLists
            .AnyAsync(x => x.Id == listId && (x.OwnerId == userId || x.SharedWith.Any(s => s.UserId == userId)), ct);
        
        if (!existingList)
            throw new KeyNotFoundException("List not found");

        var newItem = new ListItem
        {
            Name = req.Name,
            ListId = listId,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Bought = false
        };
        
        await context.ListItems.AddAsync(newItem, ct);
        await context.SaveChangesAsync(ct);

        var response = new ListItemResponse
        {
            Id = newItem.Id,
            Name = newItem.Name,
            Bought = newItem.Bought,
            CreatedByUser = new UserSummary { Id = userId, Name = userName },
            BoughtByUser = null,
            BoughtAt = null,
            UpdatedAt = newItem.UpdatedAt,
            CreatedAt = newItem.CreatedAt,
            ListId = newItem.ListId
        };

        await hubContext.Clients.Group($"list-{listId}").SendAsync("ItemAdded", response, ct);

        return response;
    }

    public async Task<ListItemResponse> UpdateItemAsync(int userId, int listId, int itemId, UpdateListItemRequest req, CancellationToken ct)
    {
        var item = await context.ListItems
            .Include(x => x.CreatedByUser)
            .Include(x => x.BoughtByUser)
            .Where(x => (x.ShoppingList.OwnerId == userId || x.ShoppingList.SharedWith.Any(s => s.UserId == userId))
                        && x.ListId == listId && x.Id == itemId)
            .FirstOrDefaultAsync(ct);

        if (item == null)
            throw new KeyNotFoundException("Item not found");

        if (req.Bought && !item.Bought)
        {
            item.BoughtByUserId = userId;
            item.BoughtAt = DateTime.UtcNow;
            item.BoughtByUser = await context.Users.FindAsync([userId], ct);
        }
        else if (!req.Bought && item.Bought)
        {
            item.BoughtByUserId = null;
            item.BoughtAt = null;
            item.BoughtByUser = null;
        }

        item.Name = req.Name;
        item.UpdatedAt = DateTime.UtcNow;
        item.Bought = req.Bought;

        await context.SaveChangesAsync(ct);

        var response = new ListItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Bought = item.Bought,
            CreatedByUser = new UserSummary { Id = item.CreatedByUser.Id, Name = item.CreatedByUser.Name },
            BoughtByUser = item.BoughtByUser != null
                ? new UserSummary { Id = item.BoughtByUser.Id, Name = item.BoughtByUser.Name }
                : null,
            BoughtAt = item.BoughtAt,
            UpdatedAt = item.UpdatedAt,
            CreatedAt = item.CreatedAt,
            ListId = item.ListId
        };

        await hubContext.Clients.Group($"list-{listId}").SendAsync("ItemUpdated", response, ct);

        return response;
    }

    public async Task DeleteItemAsync(int userId, int listId, int itemId, CancellationToken ct)
    {
        var item = await context.ListItems
            .Where(x => x.ListId == listId && x.Id == itemId &&
                        (x.ShoppingList.OwnerId == userId || x.ShoppingList.SharedWith.Any(s => s.UserId == userId)))
            .FirstOrDefaultAsync(ct);

        if (item == null)
            throw new KeyNotFoundException("Item not found");

        context.ListItems.Remove(item);
        await context.SaveChangesAsync(ct);

        await hubContext.Clients.Group($"list-{listId}").SendAsync("ItemDeleted", new { listId, itemId }, ct);
    }
}