using ShoppingList_WebAPI.DTOs.UserDTOs;

namespace ShoppingList_WebAPI.Data;

public static class DemoData
{
    public static IReadOnlyList<UserResponse> Users { get; } =
    [
        new() { Id = 1, Name = "Anna Berger", Email = "anna.berger@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 2, Name = "Jonas Klein", Email = "jonas.klein@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 3, Name = "Lena Fischer", Email = "lena.fischer@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 4, Name = "Tim Wagner", Email = "tim.wagner@demo.ourlist.app", RoleId = 2, RoleName = "admin" },
        new() { Id = 5, Name = "Sophie Krüger", Email = "sophie.krueger@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 6, Name = "Paul Neumann", Email = "paul.neumann@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 7, Name = "Mia Schulz", Email = "mia.schulz@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 8, Name = "Felix Hoffmann", Email = "felix.hoffmann@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 9, Name = "Laura Zimmermann", Email = "laura.zimmermann@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 10, Name = "David Braun", Email = "david.braun@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 11, Name = "Julia Vogel", Email = "julia.vogel@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 12, Name = "Max Richter", Email = "max.richter@demo.ourlist.app", RoleId = 2, RoleName = "admin" },
        new() { Id = 13, Name = "Hannah Lorenz", Email = "hannah.lorenz@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 14, Name = "Leon Weber", Email = "leon.weber@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 15, Name = "Emilia Koch", Email = "emilia.koch@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 16, Name = "Noah Schmitt", Email = "noah.schmitt@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 17, Name = "Marie Peters", Email = "marie.peters@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 18, Name = "Ben Möller", Email = "ben.moeller@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 19, Name = "Clara Hartmann", Email = "clara.hartmann@demo.ourlist.app", RoleId = 1, RoleName = "user" },
        new() { Id = 20, Name = "Elias Sommer", Email = "elias.sommer@demo.ourlist.app", RoleId = 2, RoleName = "admin" },
    ];
}
