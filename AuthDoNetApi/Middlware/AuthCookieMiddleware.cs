using AuthDoNetApi.AuthService;
using System.Security.Claims;

namespace AuthDoNetApi.Middlware;

public class AuthCookieMiddleware
{
    private readonly RequestDelegate _next;

    public AuthCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        if (context.Request.Cookies.TryGetValue("auth", out var authToken))
        {
            var userName = authService.UnprotectUser(authToken);

            if (!string.IsNullOrEmpty(userName))
            {
                var claims = new[]
                {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, "User")
            };

                var identity = new ClaimsIdentity(claims, "auth-cookie");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}
