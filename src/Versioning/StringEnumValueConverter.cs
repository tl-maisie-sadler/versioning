using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Versioning;

public abstract class StringEnumValueConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => throw new NotImplementedException();

    public override void Write(
        Utf8JsonWriter writer,
        TEnum objectToWrite,
        JsonSerializerOptions options)
    {
        objectToWrite = ConvertValue(objectToWrite);

        var enumString = TypeDescriptor.GetConverter(objectToWrite).ConvertTo(objectToWrite, typeof(string))?.ToString();
        writer.WriteStringValue(enumString);
    }

    private readonly List<(string version, TEnum valueToMap, TEnum mappedValue)> _mappings = new();

    protected void BeforeVersion(string version, TEnum valueToMap, TEnum mappedValue)
    {
        _mappings.Add((version, valueToMap, mappedValue));
    }

    private TEnum ConvertValue(TEnum value)
    {
        var requestVersion = Activity.Current.GetTlVersion();
        foreach (var (version, valueToMap, mappedValue) in _mappings)
        {
            var toComparisonResult = VersionExtensions.CompareVersions(version, requestVersion);
            var ruleApplies = toComparisonResult > 0;
            if (ruleApplies && value.Equals(valueToMap))
            {
                return mappedValue;
            }
        }

        return value;
    }
}
