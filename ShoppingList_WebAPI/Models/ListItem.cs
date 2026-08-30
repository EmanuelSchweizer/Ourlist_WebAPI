namespace ShoppingList_WebAPI.Models;

public class ListItem
{
    public int Id { get; set; }
    public required string Name  { get; set; }
        
    public required int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    
    public bool Bought { get; set; } = false;
    
    public int? BoughtByUserId { get; set; }
    public User? BoughtByUser { get; set; }
    public DateTime? BoughtAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public int ListId { get; set; }

    public ShoppingList ShoppingList { get; set; } = null!;
}