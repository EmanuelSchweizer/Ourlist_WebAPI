using Microsoft.EntityFrameworkCore;
using ShoppingList_WebAPI.Models;

namespace ShoppingList_WebAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users =>  Set<User>();
    public DbSet<ShoppingList> ShoppingLists =>  Set<ShoppingList>();
    public DbSet<SharedList> SharedLists =>  Set<SharedList>();
    public DbSet<Role> Roles =>  Set<Role>();
    public DbSet<ListItem> ListItems =>  Set<ListItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        //unique listId + userId
        modelBuilder.Entity<SharedList>()
            .HasIndex(sl => new { sl.ListId, sl.UserId })
            .IsUnique();
        
        // Foreign Key Constraints
        modelBuilder.Entity<ShoppingList>()
            .HasOne(l => l.Owner)
            .WithMany(u => u.OwnedLists)
            .HasForeignKey(l => l.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ListItem>()
            .HasOne(i => i.ShoppingList)
            .WithMany(l => l.Items)
            .HasForeignKey(i => i.ListId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ListItem>()
            .HasOne(i => i.CreatedByUser)
            .WithMany()
            .HasForeignKey(i => i.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<ListItem>()
            .HasOne(i => i.BoughtByUser)
            .WithMany()
            .HasForeignKey(i => i.BoughtByUserId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<SharedList>()
            .HasOne(sl => sl.ShoppingList)
            .WithMany(l => l.SharedWith)
            .HasForeignKey(sl => sl.ListId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.TokenHash)
            .IsUnique();
    }
}