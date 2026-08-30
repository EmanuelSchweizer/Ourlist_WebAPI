using System.Text;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.Middleware;
using ShoppingList_WebAPI.Services;
using ShoppingList_WebAPI.Extensions;
using ShoppingList_WebAPI.Services.ListItems;
using ShoppingList_WebAPI.Services.SharedLists;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ShoppingList_WebAPI.Services.Roles;

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://+:{port}");

//Configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

//Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ISystemUserProvider, SystemUserProvider>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();
builder.Services.AddScoped<IListItemsService, ListItemsService>();
builder.Services.AddScoped<ISharedListService, SharedListService>();
builder.Services.AddScoped<IRolesService,  RolesService>();
builder.Services.AddDbContext<AppDbContext>(opt 
    => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole("admin"));
});
builder.Services.AddCustomRateLimiting();

var app = builder.Build();

//Init data on empty database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate(); 
    SeedData.Initialize(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();  
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();