using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ShoppingList_WebAPI.Data;
using ShoppingList_WebAPI.Extensions;
using ShoppingList_WebAPI.Hubs;
using ShoppingList_WebAPI.Middleware;

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://+:{port}");

// Configuration
builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

// Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.AddSignalRWithCors();
builder.Services.AddDbContext<AppDbContext>(opt
    => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCustomRateLimiting();

var app = builder.Build();

// Init data on empty database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    SeedData.Initialize(context);
}

// Request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
}

app.UseCors("SignalRPolicy");
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHub<ShoppingListHub>("/hubs/shoppingList")
    .DisableRateLimiting();

app.Run();