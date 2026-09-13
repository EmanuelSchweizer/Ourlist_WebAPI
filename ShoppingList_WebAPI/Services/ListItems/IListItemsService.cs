using ShoppingList_WebAPI.DTOs.ListItemDTOs;

namespace ShoppingList_WebAPI.Services.ListItems;

public interface IListItemsService
{
    Task<ListItemResponse> AddListItemAsync(int userId, string userName, int listId, AddListItemRequest req, CancellationToken ct);
    Task<ListItemResponse> UpdateItemAsync(int userId, int listId, int itemId, UpdateListItemRequest req, CancellationToken ct);
    Task DeleteItemAsync(int userId, int listId, int itemId, CancellationToken ct);
}