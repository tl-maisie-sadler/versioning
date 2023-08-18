public class VersionHeaderMiddleware
{
    private RequestDelegate _next;

    public VersionHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Tl-Version", out var version))
            context.Response.Headers.Add("Tl-Version", version.ToString());

        await _next(context);
    }
}
