using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShoppingList_WebAPI.DTOs.SharedListDTOs;
using ShoppingList_WebAPI.Services.SharedLists;
using ShoppingList_WebAPI.Extensions;

namespace ShoppingList_WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SharedListController(ISharedListService service) : ControllerBase
{
    [HttpGet("{listId}")]
    public async Task<ActionResult> GetSharedUserList(int listId, CancellationToken ct = default)
    {
        var ownerId = User.GetUserId();
        var response = await service.GetSharedUserList(ownerId, listId, ct);
        return Ok(response);
    }
    
    [EnableRateLimiting("strict")]
    [HttpPost("{listId}")]
    public async Task<ActionResult> ShareList(int listId, SharedListRequest req, CancellationToken ct = default)
    {
        var ownerId = User.GetUserId();
        await service.ShareListAsync(ownerId, listId, req, ct);
        return NoContent();
    }

    [HttpDelete("{listId}/{sharedUserId}")]
    public async Task<ActionResult> UnShareList(int listId, int sharedUserId, CancellationToken ct = default)
    {
        var ownerId = User.GetUserId();
        await service.UnShareListAsync(ownerId, listId, sharedUserId, ct);
        return NoContent();
    }
}