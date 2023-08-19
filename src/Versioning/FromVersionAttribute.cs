namespace Versioning;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FromVersionAttribute : TrueLayerVersionAttribute
{
    public string MinimumVersion { get; }

    public FromVersionAttribute(string minimumVersion)
    {
        MinimumVersion = minimumVersion;
    }

    public override bool ShouldSkipForVersion(string requestVersion)
    {
        var fromComparisonResult = VersionExtensions.CompareVersions(MinimumVersion, requestVersion);
        return fromComparisonResult > 0;
    }
}
