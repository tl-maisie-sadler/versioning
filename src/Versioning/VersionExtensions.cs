using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Versioning;

internal static class VersionExtensions
{
    internal static string GetTlVersion(this HttpRequest httpRequest)
    {
        if (httpRequest.Headers.TryGetValue("Tl-Version", out var version))
        {
            if (DateOnly.TryParseExact(version.ToString(), "yyyy-MM-dd", out var date))
                return KnownTlVersions.Instance.FindVersionForDate(date);

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
        var requestVersion = activity?.GetTagItem("version") as string;

        var requestVersionOrder = KnownTlVersions.Instance.GetOrder(requestVersion ?? throw new InvalidOperationException("Activity version not set"));
        var versionToCompareOrder = KnownTlVersions.Instance.GetOrder(versionToCompare);

        if (requestVersionOrder < versionToCompareOrder) return -1;
        if (requestVersion == versionToCompare) return 0;
        return 1;
    }
}
