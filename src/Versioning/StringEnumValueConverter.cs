using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Versioning;

public abstract class StringEnumValueConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private record MapEnumBeforeVersionRule(string Version, TEnum Value);

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

    private readonly Dictionary<TEnum, List<MapEnumBeforeVersionRule>> _mappings = new();

    protected void MapValueBeforeVersion(string version, TEnum valueToMap, TEnum mappedValue)
    {
        if (!_mappings.ContainsKey(valueToMap)) _mappings[valueToMap] = new();

        _mappings[valueToMap].Add(new(version, mappedValue));
    }

    private TEnum ConvertValue(TEnum value)
    {
        var requestVersion = Activity.Current.GetTlVersion();
        if (_mappings.TryGetValue(value, out var rules))
        {
            foreach (var rule in rules)
            {
                var toComparisonResult = VersionExtensions.CompareVersions(rule.Version, requestVersion);
                var ruleApplies = toComparisonResult > 0;
                if (ruleApplies)
                {
                    return rule.Value;
                }
            }
        }

        return value;
    }
}
