using AuthDoNetApi.AuthService;
using AuthDoNetApi.Middlware;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IAuthService, AuthService>();

var app = builder.Build();
app.UseMiddleware<AuthCookieMiddleware>();

// Login endpoint: sets encrypted cookie
app.MapGet("/login", (HttpContext ctx, IAuthService authService) =>
{
    var protectedToken = authService.ProtectUser("zakaria");
    ctx.Response.Cookies.Append("auth", protectedToken);

    return Results.Ok("Login success");
});

// Protected endpoint: reads user from HttpContext.User
app.MapGet("/username", (HttpContext ctx) =>
{
    if (!ctx.User.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();

    var userName = ctx.User.Identity?.Name;

    return Results.Ok(userName);
});

app.Run();


