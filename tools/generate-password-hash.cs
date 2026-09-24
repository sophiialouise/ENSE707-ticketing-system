#:sdk Microsoft.NET.Sdk.Web

using Microsoft.AspNetCore.Identity;
using System.Text;

Console.Write("Enter password to hash: ");

var password = ReadPassword();

if (string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Password cannot be empty.");
    return;
}

var hasher = new PasswordHasher<object>();

var hash = hasher.HashPassword(
    new object(),
    password);

Console.WriteLine();
Console.WriteLine("Generated password hash:");
Console.WriteLine(hash);

static string ReadPassword()
{
    var password = new StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password.Length--;
            }

            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }

    return password.ToString();
}