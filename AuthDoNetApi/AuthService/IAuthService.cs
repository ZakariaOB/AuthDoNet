namespace AuthDoNetApi.AuthService;

public interface IAuthService
{
    string ProtectUser(string userName);
    string? UnprotectUser(string protectedToken);
}
