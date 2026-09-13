using Microsoft.EntityFrameworkCore;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.DTOs.SharedListDTOs;
using ShoppingList_WebAPI.Models;

namespace ShoppingList_WebAPI.Services.SharedLists;

public class SharedListService(AppDbContext context) : ISharedListService
{
    public async Task<SharedUserResponse> GetSharedUserList(int ownerId, int listId, CancellationToken ct)
    {
        var listExist = await context.ShoppingLists
            .AnyAsync(x => x.Id == listId && x.OwnerId == ownerId, ct);
        if (!listExist)
            throw new KeyNotFoundException("List not found");

        var sharedUsers = await context.SharedLists.Select(x => new SharedUser
        {
            Id = x.UserId,
            Name = x.User.Name,
            Email = x.User.Email,
        }).ToListAsync(ct);

        return new SharedUserResponse
        {
            listId = listId,
            SharedUsers = sharedUsers,
        };
    }
    
    public async Task ShareListAsync(int ownerId, int listId, SharedListRequest req, CancellationToken ct)
    {
        var listExist = await context.ShoppingLists
            .AnyAsync(x => x.Id == listId && x.OwnerId == ownerId, ct);
        if (!listExist)
            throw new KeyNotFoundException("List not found");
        
        var sharedUser = await context.Users.FirstOrDefaultAsync(x => x.Email == req.Email, ct);
        if (sharedUser == null)
            throw new KeyNotFoundException("Email not found");
        
        if (sharedUser.Id == ownerId)
            throw new InvalidOperationException("You cannot share a list with yourself");
        
        var recordExist = await context.SharedLists.AnyAsync(x => x.ListId == listId && x.UserId == sharedUser.Id, ct);
        if (recordExist)
            throw new InvalidOperationException("List is already shared with this user");

        var newRecord = new SharedList
        {
            UserId = sharedUser.Id,
            ListId = listId
        };

        context.SharedLists.Add(newRecord);
        await context.SaveChangesAsync(ct);
    }

    public async Task UnShareListAsync(int ownerId, int listId, int sharedUserId, CancellationToken ct)
    {
        var record = await context.SharedLists
            .FirstOrDefaultAsync(x => 
                x.ListId == listId && x.UserId == sharedUserId && x.ShoppingList.OwnerId == ownerId, ct);
        
        if (record == null)
            throw new KeyNotFoundException("Shared record not found");
        
        context.Remove(record);
        await context.SaveChangesAsync(ct);
    }
}