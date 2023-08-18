using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<InternalHandler>();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
    {
        Modifiers = { DetectIgnoreDataMemberAttribute }
    };
});

static void DetectIgnoreDataMemberAttribute(JsonTypeInfo typeInfo)
{
    if (typeInfo.Kind != JsonTypeInfoKind.Object)
        return;

    foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
    {
        if (propertyInfo.AttributeProvider is ICustomAttributeProvider provider)
        {
            var fromVersion = provider
                .GetCustomAttributes(typeof(FromVersionAttribute), inherit: true)
                .FirstOrDefault();
            if (fromVersion == null) continue;
            if (fromVersion is not FromVersionAttribute fromVersionAttribute) continue;

            propertyInfo.ShouldSerialize = (obj, value) =>
            {
                // obj = class
                // value = property
                var comparisonResult = Activity.Current.CompareToTlVersion(fromVersionAttribute.MinimumVersion);
                return comparisonResult >= 0;
            };
        }
    }
}

KnownTlVersions.Instance.Register(Versions.V_2023_01_31);
KnownTlVersions.Instance.Register(Versions.V_2023_06_30);

var app = builder.Build();

app.UseMiddleware<VersionHeaderMiddleware>();

app.MapGet("/", async (HttpContext httpContext) =>
{
    var handler = httpContext.RequestServices.GetRequiredService<InternalHandler>();
    var result = await handler.Invoke();

    return new Response(result.param1, result.param2, result.number1);
});

app.Run();

public partial class Program { }

internal static class Versions
{
    internal const string V_2023_01_31 = "2023-01-31";
    internal const string V_2023_06_30 = "2023-06-30";
}

public class KnownTlVersions
{
    public static KnownTlVersions Instance { get; } = new();

    private readonly Dictionary<string, int> _versions = new();
    private int _order = 0;

    public void Register(string version)
    {
        if (!DateOnly.TryParseExact(version, "yyyy-MM-dd", out _)) throw new InvalidOperationException("Date is not valid");

        _versions[version] = _order++;
    }

    public int GetOrder(string version)
    {
        if (_versions.TryGetValue(version, out var order))
            return order;

        throw new InvalidOperationException("Version is not registered");
    }
}

public record Response
{
    public Response(string parameter, string? anotherParameter, int? aNumber)
    {
        this.parameter = parameter;
        this.another_parameter = anotherParameter;
        this.a_number = aNumber;
    }

    public string parameter { get; set; }

    [FromVersion(Versions.V_2023_06_30)]
    public int? a_number { get; set; }

    [FromVersion(Versions.V_2023_06_30)]
    public string? another_parameter { get; set; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FromVersionAttribute : Attribute
{
    public string MinimumVersion { get; }

    public FromVersionAttribute(string minimumVersion)
    {
        MinimumVersion = minimumVersion;
    }
}
