namespace ShoppingList_WebAPI.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-API-Key";

    private static readonly string[] ExemptPaths =
    {
        "/openapi",
        "/scalar",
        "/swagger",
        "/hubs"
    };

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        var path = context.Request.Path.Value ?? "";

        if (ExemptPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

        var apiKey = context.Request.Headers[API_KEY_HEADER].ToString();
        var validApiKey = configuration["ApiKey"];

        if (string.IsNullOrEmpty(apiKey) || apiKey != validApiKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Unauthorized: Invalid API Key" });
            return;
        }

        await _next(context);
    }
}
