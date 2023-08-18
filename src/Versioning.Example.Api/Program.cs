using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<InternalHandler>();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

app.UseMiddleware<VersionHeaderMiddleware>();

app.MapGet("/", async (HttpContext httpContext) =>
{
    var handler = httpContext.RequestServices.GetRequiredService<InternalHandler>();

    var version = httpContext.Request.GetTlVersion();
    var result = await handler.Invoke();

    return new Response(result.param1, result.param2);
});

app.Run();

public partial class Program { }

[FromVersionConverter<Response>()]
public record Response
{
    public Response(string parameter, string? anotherParameter = null)
    {
        this.parameter = parameter;
        this.another_parameter = anotherParameter;
    }

    public string parameter { get; set; }

    [FromVersion("2023-06-30")]
    public string? another_parameter { get; set; }
}

public class FromVersionConverter<T> : JsonConverter<T>
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => throw new InvalidOperationException("Model used for serialization only");

    public override void Write(
        Utf8JsonWriter writer,
        T? objectToWrite,
        JsonSerializerOptions options)
    {
        if (objectToWrite == null)
        {
            writer.WriteNull("ohh");
            return;
        }
        writer.WriteStartObject();
        foreach (var property in objectToWrite.GetType().GetProperties())
        {
            var customAttr = property.CustomAttributes
                .Where(s => s.AttributeType == typeof(FromVersionAttribute))
                .FirstOrDefault();

            void WriteProperty()
            {
                writer.WritePropertyName(property.Name);

                var valueConverter = (JsonConverter<string>)options.GetConverter(property.PropertyType!);
                valueConverter.Write(writer, property.GetValue(objectToWrite).ToString(), options);
            }
            if (customAttr != null)
            {
                var tag = Activity.Current?.GetTagItem("version")?.ToString();
                if (tag == null
                    || tag != customAttr.ConstructorArguments[0].Value?.ToString())
                    WriteProperty();
            }
            else
            {
                WriteProperty();
            }
        }
        writer.WriteEndObject();
    }
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

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FromVersionConverterAttribute<T> : JsonConverterAttribute
{
    public override JsonConverter? CreateConverter(Type _)
    {
        return new FromVersionConverter<T>();
    }
}
