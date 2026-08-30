using ShoppingList_WebAPI.DTOs.ListItemDTOs;
using ShoppingList_WebAPI.Models;

namespace ShoppingList_WebAPI.DTOs.ListDTOs;

public class ShoppingListResponse
{
    public int Id  { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; }
    public string OwnerEmail { get; set; }
    public List<ListItemResponse> Items { get; set; }
}