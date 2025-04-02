using IdentityManagement.UserUtilities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace IdentityManagement.Database;

public class Database
{
    public static string UserHash(string username)
         => Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(username)));

    public async Task<User> GetUserAsync(string username)
    {
        string hash = UserHash(username);
        if (!File.Exists(hash))
        {
            return null;
        }

        await using var reader = File.OpenRead(hash);
        return await JsonSerializer.DeserializeAsync<User>(reader);
    }

    public async Task PutAsync(User user)
    {
        string hash = UserHash(user.Username);
        await using var writer = File.OpenWrite(hash);
        await JsonSerializer.SerializeAsync(writer, user);
    }
}