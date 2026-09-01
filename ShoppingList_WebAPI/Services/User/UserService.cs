using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.DTOs.UserDTOs;
using ShoppingList_WebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using ShoppingList_WebAPI.DTOs.RefreshTokenDTOs;

namespace ShoppingList_WebAPI.Services;

public class UserService(AppDbContext context, IConfiguration config, ISystemUserProvider systemUserProvider) : IUserService
{
    public async Task<SignInUserResponse> SignUpAsync(SignUpUserRequest req, CancellationToken ct)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == req.Email, ct);
        
        if(user != null)
            throw new InvalidOperationException("User with the same email already exists");
        
        var role = await context.Roles.FirstOrDefaultAsync(x => x.Name == "user", ct);
        
        if(role == null)
            throw new KeyNotFoundException("Role not found");
        
        var newUser = new User
        {
            Name = req.Name,
            Email = req.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            RoleId = role.Id,
        };
        context.Users.Add(newUser);
        await context.SaveChangesAsync(ct);
        
        var token = GenerateJwtToken(newUser, role);
        var refreshToken = await CreateRefreshTokenAsync(newUser.Id, ct);

        return new SignInUserResponse
        {
            User = new UserResponse
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                RoleId = newUser.RoleId,
                RoleName = role.Name
            },
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<SignInUserResponse> SignInAsync(SignInUserRequest req, CancellationToken ct)
    {
        var user = await context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == req.Email, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.Password))
            throw new UnauthorizedAccessException("Invalid credentials");
        
        var token = GenerateJwtToken(user, user.Role);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, ct);

        return new SignInUserResponse
        {
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role.Name
            },
            Token = token,
            RefreshToken = refreshToken
        };
    }
    
    public async Task<SignInUserResponse> ResolveOrCreateUserAsync(ResolveUserRequest req, CancellationToken ct)
    {
        var user = await context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == req.Email, ct);

        if (user == null)
        {
            var role = await context.Roles.FirstOrDefaultAsync(x => x.Name == "user", ct);
            if (role == null)
                throw new KeyNotFoundException("Role not found");

            user = new User
            {
                Name = req.Name,
                Email = req.Email,
                // OAuth-only account: random hash
                Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                RoleId = role.Id
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(ct);
            user.Role = role;
        }

        var token = GenerateJwtToken(user, user.Role);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, ct);

        return new SignInUserResponse
        {
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role.Name
            },
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<UserResponse> UpdateUserAsync(int adminUserId, int userId, UpdateUserRequest req, CancellationToken ct)
    {
        var user = await context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == userId, ct);
    
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var emailTaken = await context.Users.AnyAsync(x => x.Email == req.Email && x.Id != userId, ct);
        if (emailTaken)
            throw new InvalidOperationException("Email is already in use");

        var role = await context.Roles.FirstOrDefaultAsync(x => x.Id == req.RoleId, ct);
        if (role is null)
            throw new KeyNotFoundException("Role not found");
        
        if(userId == adminUserId && role.Id != user.RoleId)
            throw new InvalidOperationException("Admin user can not change his own role");
    
        user.Name = req.Name;
        user.Email = req.Email;
        user.RoleId = req.RoleId;
        user.Role = role;
    
        await context.SaveChangesAsync(ct);
        
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role.Name
        };
    }

    public async Task UpdatePasswordAsync(int userId, UpdateUserPasswordRequest req, CancellationToken ct)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.Password = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await context.SaveChangesAsync(ct);
        
        await RevokeAllRefreshTokensAsync(userId, ct);
    }

    public async Task DeleteUserAsync(int adminUserId, int userId, CancellationToken ct)
         {
             if (userId == adminUserId)
                 throw new InvalidOperationException("Admin user can't delete his own user");
     
             if (userId == systemUserProvider.DeletedUserId)
                 throw new InvalidOperationException("Cannot delete the system placeholder user");
     
             var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
             if (user == null)
                 throw new KeyNotFoundException("User not found");
     
             //listItems of foreign lists set createdByUserId to detetedUserId placeholder
             var createdItems = await context.ListItems
                 .Where(i => i.CreatedByUserId == userId && i.ShoppingList.OwnerId != userId)
                 .ToListAsync(ct);
     
             foreach (var item in createdItems)
                 item.CreatedByUserId = systemUserProvider.DeletedUserId;
     
             var boughtItems = await context.ListItems
                 .Where(i => i.BoughtByUserId == userId && i.ShoppingList.OwnerId != userId)
                 .ToListAsync(ct);
     
             foreach (var item in boughtItems)
                 item.BoughtByUserId = systemUserProvider.DeletedUserId;
     
             await RevokeAllRefreshTokensAsync(userId, ct);
             context.Users.Remove(user);
             await context.SaveChangesAsync(ct);
         }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var tokenHash = HashToken(req.RefreshToken);
        
        var existingToken = await context.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
        
        if (existingToken == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        if (existingToken.RevokedAt != null)
        {
            //already used Token => suspicion of theft
            //revoke all of the user's tokens
            
            await RevokeAllRefreshTokensAsync(existingToken.UserId, ct);
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (existingToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token is expired");
        
        
        // revoke old token & create new token
        existingToken.RevokedAt = DateTime.UtcNow;
        
        var newRefreshToken = await CreateRefreshTokenAsync(existingToken.UserId, ct);
        var newAcessToken = GenerateJwtToken(existingToken.User, existingToken.User.Role);

        return new RefreshTokenResponse
        {
            Token = newAcessToken,
            RefreshToken = newRefreshToken
        };
    }
    
    public async Task LogoutAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var tokenHash = HashToken(req.RefreshToken);

        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (token == null)
            return;

        token.RevokedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<UserResponse>> GetAllUsersAsync(CancellationToken ct)
    {
        var users = await context.Users
            .Include(x => x.Role)
            .Select(x => new UserResponse
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                RoleId = x.RoleId,
                RoleName = x.Role.Name
            })
            .ToListAsync(ct);

        return users;
    }

    //HELPERS
    
    private string GenerateJwtToken(User user, Role role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = config["Jwt:Secret"];
        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("JWT Secret not configured");
        
        var key = Encoding.ASCII.GetBytes(secret);
    
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role.Name),
                new Claim(ClaimTypes.Name, user.Name)
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
    
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
    
    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
    
    private async Task<string> CreateRefreshTokenAsync(int userId, CancellationToken ct)
    {
        var refreshToken = GenerateRefreshToken();

        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);

        return refreshToken;
    }
    
    private async Task RevokeAllRefreshTokensAsync(int userId, CancellationToken ct)
    {
        var tokens = await context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var t in tokens)
            t.RevokedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }
}