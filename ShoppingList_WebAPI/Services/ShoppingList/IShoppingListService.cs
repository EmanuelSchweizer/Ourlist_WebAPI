using ShoppingList_WebAPI.DTOs.ListDTOs;

namespace ShoppingList_WebAPI.Services;

public interface IShoppingListService
{
    Task<IReadOnlyList<ShoppingListResponse>> GetAllListsAsync(int userId, CancellationToken ct);
    Task<ShoppingListResponse> GetListAsync(int userId, int listId, CancellationToken ct);
    Task<ShoppingListResponse> CreateListAsync(int userId, CreateShoppingListRequest req, CancellationToken ct);
    Task<ShoppingListResponse> UpdateListAsync(int userId, int listId, UpdateShoppingListRequest req, CancellationToken ct);
    Task DeleteListAsync(int userId, int listId, CancellationToken ct);
}