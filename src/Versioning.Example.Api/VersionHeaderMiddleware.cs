using System.Diagnostics;

public class VersionHeaderMiddleware
{
    private RequestDelegate _next;

    public VersionHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            var version = context.Request.GetTlVersion();
            Activity.Current?.SetTag("version", version);
            context.Response.Headers.Add("Tl-Version", version);

            await _next(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

internal static class VersionExtensions
{
    internal static string GetTlVersion(this HttpRequest httpRequest)
    {
        if (httpRequest.Headers.TryGetValue("Tl-Version", out var version))
            return version.ToString();

        throw new InvalidOperationException("No version set");
    }
}
