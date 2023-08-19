namespace Versioning;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FromVersionAttribute : Attribute
{
    public string MinimumVersion { get; }

    public FromVersionAttribute(string minimumVersion)
    {
        MinimumVersion = minimumVersion;
    }
}
