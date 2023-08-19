using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Versioning;

internal class VersionHeaderMiddleware
{
    private RequestDelegate _next;
    private readonly ILogger<VersionHeaderMiddleware> _logger;

    public VersionHeaderMiddleware(
        RequestDelegate next,
        ILogger<VersionHeaderMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            var version = context.Request.GetTlVersion();
            Activity.Current.SetTlVersion(version);
            context.Response.Headers.Add("Tl-Version", version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Suppressing error with versioning: " + ex.Message);
            _logger.LogError(ex.StackTrace);
        }

        await _next(context);
    }
}
