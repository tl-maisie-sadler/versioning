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

    internal static string GetTlVersion(this Activity? activity)
    {
        return activity?.GetTagItem("version") as string
            ?? throw new InvalidOperationException("Activity version not set");
    }

    internal static int CompareVersions(string dateOneString, string dateTwoString)
    {

        if (!DateOnly.TryParseExact(dateOneString, "yyyy-MM-dd", out var dateOne)
            || !DateOnly.TryParseExact(dateTwoString, "yyyy-MM-dd", out var dateTwo))
            throw new InvalidOperationException("Version is not registered");

        return dateOneString.CompareTo(dateTwoString);
    }
}
