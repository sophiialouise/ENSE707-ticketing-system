#:property PublishAot=false

// task 7 performance test (TC26, TC42)
// start a fresh copy of the web app first, then run:
// dotnet run --file tools/perf-test.cs -- http://localhost:5122

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

var baseUrl = (args.Length > 0 ? args[0] : "http://localhost:5122").TrimEnd('/');

var password =
    Environment.GetEnvironmentVariable("TICKETING_MANAGER_PASSWORD")
    ?? ReadPassword("Manager password: ");

var handler = new HttpClientHandler
{
    CookieContainer = new CookieContainer(),
    AllowAutoRedirect = false
};

using var client = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

// log in as manager because import is manager-only
var loginToken = await GetToken("/Account/Login");

var login = await client.PostAsync(
    "/Account/Login",
    new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["username"] = "manager",
        ["password"] = password,
        ["__RequestVerificationToken"] = loginToken
    }));

if (login.StatusCode != HttpStatusCode.Redirect)
{
    Console.WriteLine("Login failed. Check the password and that the app is running.");
    return;
}

Console.WriteLine($"Target: {baseUrl}  ({DateTime.Now:yyyy-MM-dd HH:mm})");
Console.WriteLine();
Console.WriteLine("Tickets | Import batch (ms) | Dashboard avg (ms) | Tickets page avg (ms)");

// first row uses the provided sample data (NFR1), later rows add generated tickets
var sample = Path.Combine("TicketingSystem", "sample_tickets.csv");
var stages = new (int total, string csv)[]
{
    (10, File.ReadAllText(sample)),
    (1_000, GenerateCsv(990)),
    (10_000, GenerateCsv(9_000)),
    (50_000, GenerateCsv(40_000))
};

foreach (var (total, csv) in stages)
{
    var importMs = await ImportCsv(csv);
    var dashboardMs = await AverageGet("/Dashboard");
    var ticketsMs = await AverageGet("/Tickets");

    Console.WriteLine(
        $"{total,7} | {importMs,17} | {dashboardMs,18} | {ticketsMs,20}");
}

Console.WriteLine();
Console.WriteLine("NFR1 target: dashboard under 3000 ms with the sample data.");

async Task<string> GetToken(string path)
{
    var html = await client.GetStringAsync(path);

    return Regex.Match(
        html,
        "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
        .Groups[1].Value;
}

async Task<long> ImportCsv(string csv)
{
    var token = await GetToken("/Import");

    using var form = new MultipartFormDataContent
    {
        { new StringContent(token), "__RequestVerificationToken" },
        { new ByteArrayContent(Encoding.UTF8.GetBytes(csv)), "file", "perf.csv" }
    };

    var timer = Stopwatch.StartNew();
    var response = await client.PostAsync("/Import", form);
    await response.Content.ReadAsStringAsync();
    timer.Stop();

    return timer.ElapsedMilliseconds;
}

// average of three full page requests, including downloading the html
async Task<long> AverageGet(string path)
{
    var times = new List<long>();

    for (var i = 0; i < 3; i++)
    {
        var timer = Stopwatch.StartNew();
        await client.GetStringAsync(path);
        timer.Stop();
        times.Add(timer.ElapsedMilliseconds);
    }

    return (long)times.Average();
}

static string GenerateCsv(int rows)
{
    var random = new Random(707);
    var priorities = new[] { "Critical", "High", "Medium", "Low" };
    var categories = new[] { "Technical Support", "Billing", "Account", "General Inquiry" };
    var channels = new[] { "Email", "Phone" };
    var staff = new[] { "Alice", "Bob", "Charlie" };

    var csv = new StringBuilder(
        "CustomerName,CustomerEmail,Category,Priority,Status," +
        "AssignedTo,Channel,Description,CreatedAt,ResolvedAt\n");

    for (var i = 0; i < rows; i++)
    {
        var created = new DateTime(2026, 7, 1).AddHours(random.Next(0, 24 * 60));
        var resolved = random.Next(2) == 0;

        csv.Append($"Load User {i},load{i}@email.com,")
            .Append($"{categories[random.Next(4)]},{priorities[random.Next(4)]},")
            .Append(resolved ? "Resolved," : "Open,")
            .Append($"{staff[random.Next(3)]},{channels[random.Next(2)]},Load test ticket,")
            .Append($"{created:yyyy-MM-dd HH:mm:ss},")
            .Append(resolved ? $"{created.AddHours(random.Next(1, 72)):yyyy-MM-dd HH:mm:ss}" : "")
            .Append('\n');
    }

    return csv.ToString();
}

static string ReadPassword(string prompt)
{
    Console.Write(prompt);
    var password = new StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return password.ToString();
        }

        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Length--;
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }
}
