using ShoppingList_WebAPI.Services;
using ShoppingList_WebAPI.Services.ListItems;
using ShoppingList_WebAPI.Services.Roles;
using ShoppingList_WebAPI.Services.SharedLists;

namespace ShoppingList_WebAPI.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISystemUserProvider, SystemUserProvider>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IShoppingListService, ShoppingListService>();
        services.AddScoped<IListItemsService, ListItemsService>();
        services.AddScoped<ISharedListService, SharedListService>();
        services.AddScoped<IRolesService, RolesService>();

        return services;
    }

    public static IServiceCollection AddSignalRWithCors(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddCors(options =>
        {
            options.AddPolicy("SignalRPolicy", policy =>
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }
}
