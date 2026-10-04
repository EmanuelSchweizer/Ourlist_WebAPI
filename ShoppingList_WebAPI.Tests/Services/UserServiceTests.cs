using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.DTOs.UserDTOs;
using ShoppingList_WebAPI.Models;
using ShoppingList_WebAPI.Services;
using ShoppingList_WebAPI.Tests.TestHelpers;

namespace ShoppingList_WebAPI.Tests.Services;

public class UserServiceTests
{
    private static readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "ein-langer-geheimer-testschluessel-mit-mindestens-32-zeichen",
            ["Jwt:Issuer"] = "test",
            ["Jwt:Audience"] = "test"
        })
        .Build();

    private readonly AppDbContext _context = TestDbContextFactory.Create();

    private async Task<(SignUpUserRequest req, UserService service)> ArrangeSignUpAsync()
    {
        var req = new SignUpUserRequest
        {
            Name = "User",
            Email = "user@example.com",
            Password = "Validpassword123."
        };

        var userRole = new Role
        {
            Name = "user",
            Id = 1
        };
        _context.Roles.Add(userRole);
        await _context.SaveChangesAsync();

        var service = new UserService(_context, _configuration, null!);

        return (req, service);
    }

    [Fact]
    public async Task SignUpAsync_WhenUserAlreadyExists_ThrowsException()
    {
        // Arrange
        var req = new SignUpUserRequest
        {
            Name = "User",
            Email = "user@example.com",
            Password = "Validpassword123."
        };

        var user = new User
        {
            Id = 1,
            Name = "User",
            Email = "user@example.com",
            Password = "Validpassword123.",
            RoleId = 1
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var service = new UserService(_context, null!, null!);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SignUpAsync(req, CancellationToken.None));

        Assert.Equal("User with the same email already exists", ex.Message);
    }

    [Fact]
    public async Task SignUpAsync_WhenUserRoleNotExists_ThrowsException()
    {
        //Arrange
        var req = new SignUpUserRequest
        {
            Name = "User",
            Email = "user@example.com",
            Password = "Validpassword123."
        };

        var service = new UserService(_context, null!, null!);

        //Act + Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.SignUpAsync(req, CancellationToken.None));

        Assert.Equal("Role not found", ex.Message);
    }

    [Fact]
    public async Task SignUpAsync_Succeeds()
    {
        //Arrange
        var (req, service) = await ArrangeSignUpAsync();

        //Act
        var result = await service.SignUpAsync(req, CancellationToken.None);

        //Assert
        Assert.NotNull(result);
        Assert.Equal("user@example.com", result.User.Email);
        Assert.Equal("User", result.User.Name);
        Assert.Equal(1, result.User.RoleId);
        Assert.Equal("user", result.User.RoleName);
    }

    [Fact]
    public async Task SignUpAsync_HashesPassword()
    {
        //Arrange
        var (req, service) = await ArrangeSignUpAsync();

        //Act
        await service.SignUpAsync(req, CancellationToken.None);

        //Assert
        var savedUser = await _context.Users.SingleAsync();
        Assert.NotEqual("Validpassword123.", savedUser.Password);
        Assert.True(BCrypt.Net.BCrypt.Verify("Validpassword123.", savedUser.Password));
    }

    [Fact]
    public async Task SignUpAsync_ReturnsValidJwt()
    {
        //Arrange
        var (req, service) = await ArrangeSignUpAsync();

        //Act
        var result = await service.SignUpAsync(req, CancellationToken.None);

        //Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Equal("test", jwt.Issuer);
        Assert.Contains("test", jwt.Audiences);
        Assert.Equal("user", jwt.Claims.First(c => c.Type == "role").Value);
        Assert.Equal("user@example.com", jwt.Claims.First(c => c.Type == "email").Value);
        Assert.Equal("User", jwt.Claims.First(c => c.Type == "unique_name").Value);
    }

    [Fact]
    public async Task SignUpAsync_StoresRefreshTokenHash()
    {
        //Arrange
        var (req, service) = await ArrangeSignUpAsync();

        //Act
        var result = await service.SignUpAsync(req, CancellationToken.None);

        //Assert
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));

        var storedToken = await _context.RefreshTokens.SingleAsync();
        Assert.Equal(result.User.Id, storedToken.UserId);
        Assert.True(DateTime.Now < storedToken.ExpiresAt);
        Assert.Null(storedToken.RevokedAt);
        Assert.NotEqual(storedToken.TokenHash, result.RefreshToken);
    }
    
    
}