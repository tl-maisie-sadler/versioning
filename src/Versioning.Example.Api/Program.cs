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
        if (propertyInfo.AttributeProvider is ICustomAttributeProvider provider
            && provider.IsDefined(typeof(FromVersionAttribute), inherit: true))
        {
            var fromVersion = provider
                .GetCustomAttributes(typeof(FromVersionAttribute), inherit: true)
                .FirstOrDefault();
            var minVersion = (fromVersion as FromVersionAttribute).MinimumVersion;

            propertyInfo.ShouldSerialize = (obj, value) =>
            {
                // obj = class
                // value = property
                return Activity.Current?.GetTagItem("version")?.ToString() != minVersion;
            };
        }
    }
}

var app = builder.Build();

app.UseMiddleware<VersionHeaderMiddleware>();

app.MapGet("/", async (HttpContext httpContext) =>
{
    var handler = httpContext.RequestServices.GetRequiredService<InternalHandler>();

    var version = httpContext.Request.GetTlVersion();
    var result = await handler.Invoke();

    return new Response(result.param1, result.param2, result.number1);
});

app.Run();

public partial class Program { }

public record Response
{
    public Response(string parameter, string? anotherParameter, int? aNumber)
    {
        this.parameter = parameter;
        this.another_parameter = anotherParameter;
        this.a_number = aNumber;
    }

    public string parameter { get; set; }

    [FromVersion("2023-06-30")]
    public int? a_number { get; set; }

    [FromVersion("2023-06-30")]
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
