using Microsoft.EntityFrameworkCore;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.Models;
using ShoppingList_WebAPI.Services.Roles;

namespace ShoppingList_WebAPI.Tests.Services;

public class RoleServiceTests
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllRolesAsync_WhenNoRoles_ReturnsEmptyList()
    {
        //Arrange
        var context = CreateContext();
        var service = new RolesService(context);
        //Act
        var result = await service.GetAllRolesAsync(CancellationToken.None);

        //Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllRolesAsync_WhenRolesExist_ReturnsRoles()
    {
        //Arrange
        var context = CreateContext();

        var roles = new List<Role>
        {
            new() { Id = 1, Name = "user" },
            new() { Id = 2, Name = "admin" },
            new() { Id = 3, Name = "demoAdmin" }
        };

        context.Roles.AddRange(roles);
        await context.SaveChangesAsync(CancellationToken.None);

        var service = new RolesService(context);

        //Act
        var result = await service.GetAllRolesAsync(CancellationToken.None);

        //Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, r => r.Name == "admin");
        Assert.Contains(result, r => r.Name == "user");
        Assert.Contains(result, r => r.Name == "demoAdmin");
        Assert.Equal(3, result.Count);
    }
    
    
}