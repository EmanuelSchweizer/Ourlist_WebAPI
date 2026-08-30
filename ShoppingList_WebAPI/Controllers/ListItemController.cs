using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingList_WebAPI.DTOs.ListItemDTOs;
using ShoppingList_WebAPI.Extensions;
using ShoppingList_WebAPI.Services.ListItems;

namespace ShoppingList_WebAPI.Controllers;

[ApiController]
[Route("shoppinglists/{listId}/items")]
[Authorize]
public class ListItemController(IListItemsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetListItems(int listId, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var response = await service.GetListItemsAsync(userId, listId, ct);
        return Ok(response);
    }
    
    [HttpPost]
    public async Task<ActionResult> AddListItem(int listId, AddListItemRequest request, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var userName = User.GetUserName();
        var response = await service.AddListItemAsync(userId, userName, listId, request, ct);
        return Ok(response);
    }
    
    [HttpPut("{itemId}")]
    public async Task<ActionResult> UpdateListItem(int listId, int itemId, UpdateListItemRequest request, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var response = await service.UpdateItemAsync(userId, listId, itemId, request, ct);
        return Ok(response);
    }
    
    [HttpDelete("{itemId}")]
    public async Task<ActionResult> DeleteListItem(int listId, int itemId, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        await service.DeleteItemAsync(userId, listId, itemId, ct);
        return NoContent();
    }
}