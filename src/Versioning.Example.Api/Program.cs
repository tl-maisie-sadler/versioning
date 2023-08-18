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
                return (int)Activity.Current.GetTlVersion()
                    != fromVersionAttribute.MinimumVersion;
            };
        }
    }
}

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

enum TlVersion
{
    // Not serialized but will be used in >= conditions
    // Important to keep dates ordered by earlier date <= later date
    Unknown = 0,
    V_2023_01_31 = 1,
    V_2023_06_30 = 2,
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

    [FromVersion((int)TlVersion.V_2023_06_30)]
    public int? a_number { get; set; }

    [FromVersion((int)TlVersion.V_2023_06_30)]
    public string? another_parameter { get; set; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FromVersionAttribute : Attribute
{
    public int MinimumVersion { get; }

    public FromVersionAttribute(int minimumVersion)
    {
        MinimumVersion = minimumVersion;
    }
}
