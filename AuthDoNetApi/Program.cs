var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();


app.MapGet("/username", (HttpContext ctx) =>
{
    var cookieHeader = ctx.Request.Headers["Cookie"].ToString(); // Full raw string

    // Split all cookies by ';' and trim spaces
    var cookies = cookieHeader
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Select(c => c.Trim());

    // Find the auth cookie
    var authCookie = cookies.FirstOrDefault(c => c.StartsWith("auth="));

    if (authCookie == null)
        return Results.Unauthorized();

    // Parse the value of auth=usr:zakaria
    var userCookie = authCookie.Split('=').Last(); // usr:zakaria
    var userName = userCookie.Split(':').Last();   // zakaria

    return Results.Ok(userName);
});


app.MapGet("/login", (HttpContext ctx) =>
{
    ctx.Response.Headers["set-cookie"] = "auth=usr:zakaria";
    return "ok";
});

app.Run();


