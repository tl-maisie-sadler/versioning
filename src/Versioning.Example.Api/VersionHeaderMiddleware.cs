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
            Activity.Current.SetTlVersion(version);
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
        {
            var date = DateOnly.ParseExact(version.ToString(), "yyyy-MM-dd");
            if (date >= new DateOnly(2023, 06, 30))
                return Versions._v_2023_06_30;

            return Versions._v_2023_01_31;
        }

        throw new InvalidOperationException("No version set");
    }

    internal static void SetTlVersion(this Activity? activity, string version)
    {
        activity?.SetTag("version", version);
    }

    internal static int CompareToTlVersion(this Activity? activity, string versionToCompare)
    {
        var requestVersion = activity?.GetTagItem("version") as string;

        var requestVersionOrder = KnownTlVersions.Instance.GetOrder(requestVersion!);
        var versionToCompareOrder = KnownTlVersions.Instance.GetOrder(versionToCompare);

        if (requestVersionOrder < versionToCompareOrder) return -1;
        if (requestVersion == versionToCompare) return 0;
        return 1;
    }
}
