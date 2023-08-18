using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<InternalHandler>();

var app = builder.Build();

app.UseMiddleware<VersionHeaderMiddleware>();

app.MapGet("/", async (HttpContext httpContext) =>
{
    var handler = httpContext.RequestServices.GetRequiredService<InternalHandler>();

    var version = httpContext.Request.GetTlVersion();
    var result = await handler.Invoke();

    if (version == "2023-02-31")
    {
        return new Response(result.param1, result.param2);
    }

    return new Response(result.param1);
});

app.Run();

public partial class Program { }


public record Response
{
    public Response(string parameter, string? anotherParameter = null)
    {
        this.parameter = parameter;
        this.another_parameter = anotherParameter;
    }

    public string parameter { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? another_parameter { get; set; }
}
