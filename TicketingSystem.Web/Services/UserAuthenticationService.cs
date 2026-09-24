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

        var supportPassword =
            configuration["DemoUsers:SupportPassword"];

        var managerPassword =
            configuration["DemoUsers:ManagerPassword"];

        if (string.IsNullOrWhiteSpace(supportPassword) ||
            string.IsNullOrWhiteSpace(managerPassword))
        {
            throw new InvalidOperationException(
                "Demo user passwords have not been configured.");
        }

        var supportUser = new UserAccount
        {
            Username = "support",
            Role = "Support"
        };

        supportUser.PasswordHash =
            _passwordHasher.HashPassword(
                supportUser,
                supportPassword);

        var managerUser = new UserAccount
        {
            Username = "manager",
            Role = "Manager"
        };

        managerUser.PasswordHash =
            _passwordHasher.HashPassword(
                managerUser,
                managerPassword);

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