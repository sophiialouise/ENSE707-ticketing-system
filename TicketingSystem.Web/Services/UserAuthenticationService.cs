using Microsoft.AspNetCore.Identity;
using TicketingSystem.Web.Models;

namespace TicketingSystem.Web.Services;

// manages the local prototype users and password verification
public class UserAuthenticationService
{
    private readonly List<UserAccount> _users;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public UserAuthenticationService(
        IConfiguration configuration,
        IPasswordHasher<UserAccount> passwordHasher)
    {
        _passwordHasher = passwordHasher;

        var supportPasswordHash =
            configuration["DemoUsers:SupportPasswordHash"];

        var managerPasswordHash =
            configuration["DemoUsers:ManagerPasswordHash"];

        if (string.IsNullOrWhiteSpace(supportPasswordHash) ||
            string.IsNullOrWhiteSpace(managerPasswordHash))
        {
            throw new InvalidOperationException(
                "Demo user password hashes have not been configured.");
        }

        var supportUser = new UserAccount
        {
            Username = "support",
            Role = "Support",
            PasswordHash = supportPasswordHash
        };

        var managerUser = new UserAccount
        {
            Username = "manager",
            Role = "Manager",
            PasswordHash = managerPasswordHash
        };

        _users = new List<UserAccount>
        {
            supportUser,
            managerUser
        };
    }

    public UserAccount? Authenticate(
        string username,
        string password)
    {
        var user = _users.FirstOrDefault(
            u => u.Username.Equals(
                username,
                StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return null;
        }

        var result =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

        return result != PasswordVerificationResult.Failed
            ? user
            : null;
    }
}