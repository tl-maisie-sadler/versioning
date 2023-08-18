var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<VersionHeaderMiddleware>();

app.MapGet("/", () =>
{
    return new {
        parameter = "value 1",
        another_parameter = "value 2",
    };
});

app.Run();

public partial class Program { }
