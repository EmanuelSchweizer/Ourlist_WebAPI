namespace ShoppingList_WebAPI.DTOs.SharedListDTOs;

public class SharedUserResponse
{
    public required int listId { get; set; }
    public required IReadOnlyList<SharedUser> SharedUsers { get; set; }
}

public class SharedUser
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}