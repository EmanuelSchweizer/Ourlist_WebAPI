using ShoppingList_WebAPI.Data;

namespace ShoppingList_WebAPI.Services;

public interface ISystemUserProvider
{
    int DeletedUserId { get; }
}

public class SystemUserProvider : ISystemUserProvider
{
    public int DeletedUserId { get; }

    public SystemUserProvider(AppDbContext context)
    {
        DeletedUserId = context.Users
            .Where(u => u.Email == SeedData.DeletedUserEmail)
            .Select(u => u.Id)
            .First();
    }
}