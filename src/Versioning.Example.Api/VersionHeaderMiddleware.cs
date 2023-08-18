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
            var (version, vs) = context.Request.GetTlVersion();
            Activity.Current?.SetTag("version", version);
            context.Response.Headers.Add("Tl-Version", vs);

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
    internal static (TlVersion, string) GetTlVersion(this HttpRequest httpRequest)
    {
        if (httpRequest.Headers.TryGetValue("Tl-Version", out var version))
        {
            var date = DateOnly.ParseExact(version.ToString(), "yyyy-MM-dd");
            if (date >= new DateOnly(2023, 06, 30))
                return (TlVersion.V_2023_06_30, version.ToString());
            
            return (TlVersion.V_2023_01_31, version.ToString());
        }

        throw new InvalidOperationException("No version set");
    }

    internal static void SetTlVersion(this Activity? activity, TlVersion version)
    {
        activity?.SetTag("version", version);
    }

    internal static TlVersion GetTlVersion(this Activity? activity)
    {
        var version = activity?.GetTagItem("version") as TlVersion?;
        return version ?? throw new Exception("Oh no, it's null!");
    }
}
