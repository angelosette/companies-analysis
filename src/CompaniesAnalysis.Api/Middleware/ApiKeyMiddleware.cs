using Microsoft.Extensions.Options;

namespace CompaniesAnalysis.Api.Middleware;

public class ApiKeyConfigOptions
{
    public const string SectionName = "ApiKey";
    public List<string> ValidKeys { get; set; } = [];
}

public class ApiKeyMiddleware
{
    private const string Header = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly IOptions<ApiKeyConfigOptions> _options;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyConfigOptions> options, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (IsExcluded(ctx.Request.Path))
        {
            await _next(ctx);
            return;
        }

        if (!ctx.Request.Headers.TryGetValue(Header, out var key))
        {
            _logger.LogWarning("Missing API key from {Ip}", ctx.Connection.RemoteIpAddress);
            ctx.Response.StatusCode = 401;
            await ctx.Response.WriteAsJsonAsync(new { error = "API key required. Use X-Api-Key header." });
            return;
        }

        if (!_options.Value.ValidKeys.Contains(key.ToString()))
        {
            _logger.LogWarning("Invalid API key from {Ip}", ctx.Connection.RemoteIpAddress);
            ctx.Response.StatusCode = 403;
            await ctx.Response.WriteAsJsonAsync(new { error = "Invalid API key." });
            return;
        }

        await _next(ctx);
    }

    private static bool IsExcluded(PathString path) =>
        path == "/" ||
        path.StartsWithSegments("/health") ||
        path.StartsWithSegments("/alive") ||
        path.StartsWithSegments("/scalar") ||
        path.StartsWithSegments("/openapi");
}
