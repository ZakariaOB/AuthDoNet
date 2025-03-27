using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Register authentication and authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        //options.LoginPath = "/login"; // optional redirect for [Authorize]
        options.Cookie.Name = "auth"; // cookie name
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("MarocainOnly", policy =>
        policy.RequireClaim("nationality", "marocain"));

var app = builder.Build();

// Enable authentication and authorization middleware
app.UseAuthentication(); // reads & validates cookie, sets HttpContext.User
app.UseAuthorization();  // applies [Authorize] attributes

// Login endpoint (simulated)
app.MapGet("/login", async (HttpContext ctx) =>
{
    // Simulate successful login (in real apps you'd validate credentials)
    var userName = "zakaria";

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, userName),
        new Claim(ClaimTypes.Role, "User"),
        new Claim("nationality", "marocain")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Ok("Login successful. Cookie is now set.");
});

// Protected endpoint: requires login
app.MapGet("/username", [Authorize] (HttpContext ctx) =>
{
    var userName = ctx.User.Identity?.Name;
    return Results.Ok(userName);
});

app.MapGet("/marocain", [Authorize(Policy = "MarocainOnly")] () =>
{
    return Results.Ok("marocain");
});

app.MapGet("/marrakech", [Authorize(Policy = "MarocainOnly")] () =>
{
    return Results.Ok("allowed to marrakech");
});

app.MapGet("/unsecure", (HttpContext ctx) =>
{
    var userName = ctx.User.Identity?.Name;
    return Results.Ok(userName);
});

app.MapPost("/login-role", async (HttpContext context, string username) =>
{
    // Fake logic: map username to roles
    var role = username.ToLower() switch
    {
        "zakaria" => "Africain",
        "john" => "American",
        _ => "Guest"
    };

    var claims = new List<Claim>
    {
        new (ClaimTypes.Name, username),
        new (ClaimTypes.Role, role)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Ok($"Logged in as {username} with role: {role}");
});

app.MapGet("/africa", [Authorize(Roles = "Africain")] () =>
{
    return Results.Ok("Hello, Africain!");
});


// Optional logout
app.MapGet("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok("Logged out successfully");
});

app.Run();
