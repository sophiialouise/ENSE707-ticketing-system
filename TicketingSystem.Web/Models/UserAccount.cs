namespace TicketingSystem.Web.Models;

// represents a local prototype user account
public class UserAccount
{
    public string Username { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}