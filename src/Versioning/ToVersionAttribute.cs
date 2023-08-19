namespace Versioning;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ToVersionAttribute : Attribute
{
    public string MaximumVersion { get; }

    public ToVersionAttribute(string maximumVersion)
    {
        MaximumVersion = maximumVersion;
    }
}
