using IdentityManagement.Database;
using IdentityManagement.UserUtilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

var app = builder.Build();

app.MapGet("/", () => "Hello Zak!");

app.MapGet("/register", async (
    string username, 
    string password,
    IPasswordHasher<User> hasher,
    Database db,
    HttpContext context) =>
{
    User user = new() { Username = username };
    user.PasswordHash = hasher.HashPassword(user, password);
    await db.PutAsync(user);

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        UserHelper.Convert(user));

    return user;
});

app.MapGet("/login", async (
    string username,
    string password,
    IPasswordHasher<User> hasher,
    Database db,
    HttpContext context) =>
{
    User user = await db.GetUserAsync(username);
    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

    if (result == PasswordVerificationResult.Failed)
    {
        return "Bad credentials!";
    }

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        UserHelper.Convert(user));

    return "Logged in Bravo !!";
});

app.Run();
