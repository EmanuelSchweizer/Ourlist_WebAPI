using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingList_WebAPI.DTOs.ListDTOs;
using ShoppingList_WebAPI.Extensions;
using ShoppingList_WebAPI.Services;

namespace ShoppingList_WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ShoppingListController(IShoppingListService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAllLists(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var response = await service.GetAllListsAsync(userId, ct);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> CreateList(CreateShoppingListRequest req, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var response = await service.CreateListAsync(userId, req, ct);
        return Ok(response);
    }
    
    [HttpPut("{listId}")]
    public async Task<ActionResult> UpdateList(int listId, UpdateShoppingListRequest req, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var response = await service.UpdateListAsync(userId, listId, req, ct);
        return Ok(response);
    }
    
    [HttpDelete("{listId}")]
    public async Task<ActionResult> DeleteList(int listId, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        await service.DeleteListAsync(userId, listId, ct);
        return NoContent();
    }
}