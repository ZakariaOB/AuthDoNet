using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection();

var app = builder.Build();

app.MapGet("/username", (HttpContext ctx, IDataProtectionProvider idp) =>
{
    var cookieHeader = ctx.Request.Headers.Cookie.ToString();

    // Split all cookies by ';' and trim spaces
    var cookies = cookieHeader
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Select(c => c.Trim());

    // Find the auth cookie
    var authCookie = cookies.FirstOrDefault(c => c.StartsWith("auth="));

    if (authCookie == null)
    {
        return Results.Unauthorized();
    }

    string userCookie = authCookie.Split('=').Last();
    string userName = userCookie.Split(':').Last();

    IDataProtector protector = idp.CreateProtector("auth-cookie");
    string unprotectedUserName = protector.Unprotect(userName);

    return Results.Ok(unprotectedUserName);
});


app.MapGet("/login", (HttpContext ctx, IDataProtectionProvider idp) =>
{
    var protector = idp.CreateProtector("auth-cookie");

    ctx.Response.Headers.SetCookie = $"auth={protector.Protect("usr:zakaria")}";

    return "loginc success";
});

app.Run();


