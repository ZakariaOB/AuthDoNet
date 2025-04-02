using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Register authentication and authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        //options.LoginPath = "/login"; // optional redirect for [Authorize]
        options.Cookie.Name = "auth"; // cookie name
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    }).AddOAuth("MockOAuth", options =>
    {
        options.ClientId = "dummy-client";
        options.ClientSecret = "dummy-secret";
        options.AuthorizationEndpoint = "https://oauth.wiremockapi.cloud/oauth/authorize";
        options.TokenEndpoint = "https://oauth.wiremockapi.cloud/oauth/token";
        options.UserInformationEndpoint = "https://oauth.wiremockapi.cloud/userinfo";

        options.CallbackPath = "/signin-oauth";

        options.SaveTokens = true;
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

        options.Events.OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var user = JsonDocument.Parse(json);

            context.RunClaimActions(user.RootElement);
        };

        // options.ClaimActions.MapAll();
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

app.MapGet("/login-mock", (HttpContext context) =>
{
    var props = new AuthenticationProperties { RedirectUri = "/" };
    return Results.Challenge(props, new[] { "MockOAuth" });
});

app.MapGet("/secure", [Authorize] (HttpContext context) =>
{
    string email = context.User.FindFirst(ClaimTypes.Email)?.Value;
    return Results.Ok($"Welcome, {email}!");
});

app.Run();
