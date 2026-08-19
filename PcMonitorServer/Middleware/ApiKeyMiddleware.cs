namespace PcMonitorServer.Middleware;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string HeaderName = "Authorization";
    private readonly string _apiKey = configuration["Monitor:ApiKey"]
        ?? throw new InvalidOperationException("Monitor:ApiKey no configurada.");

    public async Task InvokeAsync(HttpContext context)
    {
        // Solo aplicar a rutas bajo /api/
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var header))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Authorization header requerido.");
            return;
        }

        var token = header.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (token != _apiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key inválida.");
            return;
        }

        await next(context);
    }
}
