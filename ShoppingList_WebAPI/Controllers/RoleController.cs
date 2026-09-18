using Microsoft.AspNetCore.Mvc;
using ShoppingList_WebAPI.DTOs.RolesDTOs;
using ShoppingList_WebAPI.Models;
using ShoppingList_WebAPI.Services.Roles;

namespace ShoppingList_WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class RoleController(IRolesService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetAllRoles(CancellationToken ct = default)
    {
        var response = await service.GetAllRolesAsync(ct);
        return Ok(response);
    }
}