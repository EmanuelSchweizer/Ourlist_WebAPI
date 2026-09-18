using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.Extensions;

namespace ShoppingList_WebAPI.Hubs;

[Authorize]
public class ShoppingListHub(AppDbContext context) : Hub
{
    public async Task JoinList(int listId)
    {
        var userId = Context.User!.GetUserId();

        var hasAccess = await context.ShoppingLists.AnyAsync(x => x.Id == listId &&
                                                                  (x.OwnerId == userId ||
                                                                   x.SharedWith.Any(y => y.UserId == userId)));

        if (!hasAccess)
            throw new HubException("You cannot join a shopping list");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"list-{listId}");
    }

    public async Task LeaveList(int listId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"list-{listId}");
    }
}