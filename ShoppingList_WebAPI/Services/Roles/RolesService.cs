using Microsoft.EntityFrameworkCore;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.DTOs.RolesDTOs;

namespace ShoppingList_WebAPI.Services.Roles;

public class RolesService(AppDbContext context) : IRolesService
{
    public async Task<IReadOnlyList<RoleResponse>> GetAllRolesAsync(CancellationToken ct)
    {
        var roles = await context.Roles.Select(x => new RoleResponse
        {
            Id = x.Id,
            Name = x.Name
        }).ToListAsync(ct);

        return roles;
    }
}