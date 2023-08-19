using Versioning;
using Versioning.Example.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<InternalHandler>();

builder.Services.AddVersioning();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseVersioning();

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
    public const string _v_2023_01_31 = "2023-01-31";
    public const string _v_2023_06_30 = "2023-06-30";
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

    [FromVersion(Versions._v_2023_06_30)]
    public int? a_number { get; set; }

    [FromVersion(Versions._v_2023_06_30)]
    public string? another_parameter { get; set; }
}
