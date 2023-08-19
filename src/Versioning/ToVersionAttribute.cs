namespace Versioning;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public abstract class TrueLayerVersionAttribute : Attribute
{
    public abstract bool ShouldSkipForVersion(string requestVersion);
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ToVersionAttribute : TrueLayerVersionAttribute
{
    public string MaximumVersion { get; }

    public ToVersionAttribute(string maximumVersion)
    {
        MaximumVersion = maximumVersion;
    }

    public override bool ShouldSkipForVersion(string requestVersion)
    {
        var toComparisonResult = VersionExtensions.CompareVersions(MaximumVersion, requestVersion);
        return toComparisonResult < 0;
    }
}
