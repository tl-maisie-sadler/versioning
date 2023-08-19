using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Versioning;

internal static class VersionExtensions
{
    internal static string GetTlVersion(this HttpRequest httpRequest)
    {
        if (httpRequest.Headers.TryGetValue("Tl-Version", out var version))
        {
            var versionString = version.ToString();
            if (DateOnly.TryParseExact(versionString, "yyyy-MM-dd", out var date))
                return versionString;

            throw new Exception($"Invalid version header '{version}'");
        }

        throw new InvalidOperationException("No version set");
    }

    internal static void SetTlVersion(this Activity? activity, string version)
    {
        // todo: can we do this without using Activity?
        activity?.SetTag("version", version);
    }

    internal static int CompareToTlVersion(this Activity? activity, string versionToCompare)
    {
        var requestVersion = activity?.GetTagItem("version") as string
            ?? throw new InvalidOperationException("Activity version not set");

        return CompareDates(requestVersion, versionToCompare);
    }

    private static int CompareDates(string dateString1, string dateString2)
    {
        if (!DateOnly.TryParseExact(dateString1, "yyyy-MM-dd", out var date1)
            || !DateOnly.TryParseExact(dateString2, "yyyy-MM-dd", out var date2))
            throw new InvalidOperationException("Version is not registered");

        return date1.CompareTo(date2);
    }
}
