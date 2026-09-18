using ShoppingList_WebAPI.DTOs.RolesDTOs;

namespace ShoppingList_WebAPI.Services.Roles;

public interface IRolesService
{
    Task<IReadOnlyList<RoleResponse>> GetAllRolesAsync(CancellationToken ct);
}