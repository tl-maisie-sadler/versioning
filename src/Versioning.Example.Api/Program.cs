var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<VersionHeaderMiddleware>();

app.MapGet("/", () =>
{
    return "Hello World!";
});

app.Run();

public partial class Program { }
