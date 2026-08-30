using System.Security.Claims;

namespace ShoppingList_WebAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user ID in token");
        
        return userId;
    }
    
    public static string GetUserName(this ClaimsPrincipal user)
    {
        var userName = user.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(userName))
            throw new UnauthorizedAccessException("Invalid or missing user name in token");
        
        return userName;
    }
}