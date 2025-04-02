namespace IdentityManagement.UserUtilities;

public class User
{
    public required string Username { get; set; }

    public string PasswordHash { get; set; }

    public List<UserClaim> UserClaims { get; set; } = new();
}
