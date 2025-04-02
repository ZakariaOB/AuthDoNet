using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace IdentityManagement.UserUtilities;

public class UserHelper
{
    public static ClaimsPrincipal Convert(User user)
    {
        var claims = new List<Claim>()
        {
            new Claim("username", user.Username)
        };

        claims.AddRange(user.UserClaims.Select(c => new Claim(c.Type, c.Value)));

        var identity = new ClaimsIdentity(
            claims, 
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}
