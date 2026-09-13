using ShoppingList_WebAPI.DTOs.SharedListDTOs;

namespace ShoppingList_WebAPI.Services.SharedLists;

public interface ISharedListService
{
    Task<SharedUserResponse> GetSharedUserList(int ownerId, int listId, CancellationToken ct);
    Task ShareListAsync(int ownerId, int listId, SharedListRequest req, CancellationToken ct);
    Task UnShareListAsync(int ownerId, int listId, int sharedUserId, CancellationToken ct);
}