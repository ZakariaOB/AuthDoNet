using Microsoft.AspNetCore.DataProtection;

namespace AuthDoNetApi.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IDataProtector _protector;

        public AuthService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("auth-cookie");
        }

        public string ProtectUser(string userName)
        {
            return _protector.Protect($"usr:{userName}");
        }

        public string UnprotectUser(string protectedToken)
        {
            try
            {
                var value = _protector.Unprotect(protectedToken);
                return value.Split(':').Last();
            }
            catch
            {
                return null;
            }
        }
    }
}
