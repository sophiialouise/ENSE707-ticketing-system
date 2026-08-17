using TicketingSystem.Models;
using TicketingSystem.Services;

namespace TicketingSystem;

class Program
{
    private static readonly TicketService _service = new();

    static void Main(string[] args)
    {
        Console.WriteLine();
        Console.WriteLine("TICKETING AND CUSTOMER SUPPORT ANALYTICS");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine("Initial ENSE707 prototype");

        bool running = true;

        while (running)
        {
            ShowMainMenu();

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    ImportTickets();
                    break;

                case "2":
                    ShowDashboard();
                    break;

                case "3":
                    ShowAllTickets();
                    break;

                case "4":
                    ShowSlaReport();
                    break;

                case "5":
                    SearchTickets();
                    break;

                case "6":
                    UpdateTicketStatus();
                    break;

                case "7":
                    ShowTicketHistory();
                    break;

                case "8":
                    LoadSampleData();
                    break;

                case "9":
                    Console.WriteLine("Exiting application.");
                    running = false;
                    break;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    static void ShowMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("MAIN MENU");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine("1. Import tickets from CSV");
        Console.WriteLine("2. View dashboard");
        Console.WriteLine("3. View all tickets");
        Console.WriteLine("4. View SLA report");
        Console.WriteLine("5. Search tickets");
        Console.WriteLine("6. Update ticket status");
        Console.WriteLine("7. View ticket history");
        Console.WriteLine("8. Load sample data");
        Console.WriteLine("9. Exit");
        Console.Write("Enter your choice: ");
    }

    static void ImportTickets()
    {
        Console.WriteLine();
        Console.WriteLine("IMPORT TICKETS");
        Console.WriteLine("----------------------------------------");
        Console.Write("Enter CSV file path: ");

        var filePath = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(filePath) ||
            !File.Exists(filePath))
        {
            Console.WriteLine("File not found. Please check the path.");
            return;
        }

        var result = _service.ImportTicketsFromCsv(filePath);

        Console.WriteLine();
        Console.WriteLine("IMPORT RESULTS");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Total records:   {result.TotalRecords}");
        Console.WriteLine($"Valid records:   {result.ValidRecords}");
        Console.WriteLine($"Invalid records: {result.InvalidRecords}");

        if (result.InvalidRecords > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Invalid record details:");

            foreach (var error in result.Errors.Take(5))
            {
                Console.WriteLine($"- {error}");
            }

            if (result.Errors.Count > 5)
            {
                Console.WriteLine(
                    $"- and {result.Errors.Count - 5} more errors");
            }
        }
    }

    static void ShowDashboard()
    {
        var dashboard = _service.GetDashboard();

        Console.WriteLine();
        Console.WriteLine("QUALITY DASHBOARD");
        Console.WriteLine("----------------------------------------");

        Console.WriteLine($"Total tickets:             {dashboard.TotalTickets}");
        Console.WriteLine($"Open tickets:              {dashboard.OpenTickets}");
        Console.WriteLine($"In progress tickets:       {dashboard.InProgressTickets}");
        Console.WriteLine($"Resolved/closed tickets:   {dashboard.ResolvedTickets}");

        Console.WriteLine();
        Console.WriteLine("QUALITY METRICS");
        Console.WriteLine("----------------------------------------");

        Console.WriteLine(
            $"Average response time:   " +
            $"{dashboard.AverageResponseTimeHours:F2} hours");

        Console.WriteLine(
            $"Average resolution time: " +
            $"{dashboard.AverageResolutionTimeHours:F2} hours");

        Console.WriteLine(
            $"SLA compliance:          " +
            $"{dashboard.SlaCompliancePercentage:F1}%");

        Console.WriteLine();
        Console.WriteLine("TICKETS BY PRIORITY");

        if (!dashboard.TicketsByPriority.Any())
        {
            Console.WriteLine("No ticket data available.");
        }
        else
        {
            foreach (var item in dashboard.TicketsByPriority)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("TICKETS BY CATEGORY");

        foreach (var item in dashboard.TicketsByCategory)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }

        Console.WriteLine();
        Console.WriteLine("TICKETS BY CHANNEL");

        foreach (var item in dashboard.TicketsByChannel)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }
    }

    static void ShowAllTickets()
    {
        var tickets = _service.GetTickets();

        Console.WriteLine();
        Console.WriteLine("ALL TICKETS");
        Console.WriteLine("----------------------------------------");

        if (!tickets.Any())
        {
            Console.WriteLine("No tickets found.");
            return;
        }

        foreach (var ticket in tickets)
        {
            Console.WriteLine();
            Console.WriteLine($"ID:          {ticket.Id}");
            Console.WriteLine($"Customer:    {ticket.CustomerName}");
            Console.WriteLine($"Category:    {ticket.Category}");
            Console.WriteLine($"Priority:    {ticket.Priority}");
            Console.WriteLine($"Status:      {ticket.Status}");
            Console.WriteLine($"Assigned to: {ticket.AssignedTo}");
            Console.WriteLine($"Channel:     {ticket.Channel}");
        }

        Console.WriteLine();
        Console.WriteLine($"Total: {tickets.Count} ticket(s)");
    }

    static void ShowSlaReport()
    {
        var report = _service.GetSlaReport();

        var reportType = report.GetType();

        var totalResolved =
            reportType.GetProperty("TotalResolved")?.GetValue(report);

        var slaMet =
            reportType.GetProperty("SlaMet")?.GetValue(report);

        var slaMissed =
            reportType.GetProperty("SlaMissed")?.GetValue(report);

        var compliance =
            reportType.GetProperty("CompliancePercentage")?.GetValue(report);

        Console.WriteLine();
        Console.WriteLine("SLA COMPLIANCE REPORT");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Total resolved: {totalResolved}");
        Console.WriteLine($"SLA met:        {slaMet}");
        Console.WriteLine($"SLA missed:     {slaMissed}");
        Console.WriteLine($"Compliance:     {compliance}%");
    }

    static void SearchTickets()
    {
        Console.WriteLine();
        Console.WriteLine("SEARCH TICKETS");
        Console.WriteLine("----------------------------------------");

        var filter = new TicketFilter();

        Console.Write("Category (press Enter to skip): ");
        var category = Console.ReadLine()?.Trim();

        if (!string.IsNullOrWhiteSpace(category))
        {
            filter.Category = category;
        }

        Console.Write("Priority (press Enter to skip): ");
        var priority = Console.ReadLine()?.Trim();

        if (!string.IsNullOrWhiteSpace(priority))
        {
            filter.Priority = priority;
        }

        Console.Write("Status (press Enter to skip): ");
        var status = Console.ReadLine()?.Trim();

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter.Status = status;
        }

        Console.Write("Assigned staff (press Enter to skip): ");
        var assignedTo = Console.ReadLine()?.Trim();

        if (!string.IsNullOrWhiteSpace(assignedTo))
        {
            filter.AssignedTo = assignedTo;
        }

        Console.Write("Channel (press Enter to skip): ");
        var channel = Console.ReadLine()?.Trim();

        if (!string.IsNullOrWhiteSpace(channel))
        {
            filter.Channel = channel;
        }

        var results = _service.GetTickets(filter);

        Console.WriteLine();
        Console.WriteLine($"Found {results.Count} matching ticket(s).");

        foreach (var ticket in results)
        {
            Console.WriteLine(
                $"{ticket.Id} | " +
                $"{ticket.CustomerName} | " +
                $"{ticket.Status} | " +
                $"{ticket.Priority} | " +
                $"{ticket.Category}");
        }
    }

    static void UpdateTicketStatus()
    {
        Console.WriteLine();
        Console.WriteLine("UPDATE TICKET STATUS");
        Console.WriteLine("----------------------------------------");

        Console.Write("Enter ticket ID: ");
        var id = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(id))
        {
            Console.WriteLine("A ticket ID is required.");
            return;
        }

        var ticket = _service.GetTicketById(id);

        if (ticket == null)
        {
            Console.WriteLine($"Ticket {id} was not found.");
            return;
        }

        Console.WriteLine($"Current status: {ticket.Status}");
        Console.WriteLine(
            "Valid statuses: Open, In Progress, Resolved, Closed");

        Console.Write("Enter new status: ");
        var newStatus = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(newStatus))
        {
            Console.WriteLine("A new status is required.");
            return;
        }

        bool updated = _service.UpdateTicketStatus(id, newStatus);

        if (!updated)
        {
            Console.WriteLine(
                "Status update failed. Please enter a valid status.");
            return;
        }

        Console.WriteLine(
            $"Ticket {id} status updated to '{ticket.Status}'.");

        if (ticket.Status == "In Progress" &&
            ticket.FirstResponseAt.HasValue)
        {
            Console.WriteLine("First response time recorded.");
        }

        if ((ticket.Status == "Resolved" ||
             ticket.Status == "Closed") &&
            ticket.ResolvedAt.HasValue)
        {
            Console.WriteLine("Resolution time recorded.");
        }
    }

    static void ShowTicketHistory()
    {
        Console.WriteLine();
        Console.WriteLine("TICKET HISTORY");
        Console.WriteLine("----------------------------------------");

        Console.Write("Enter ticket ID: ");
        var id = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(id))
        {
            Console.WriteLine("A ticket ID is required.");
            return;
        }

        var history = _service.GetTicketHistory(id);

        if (!history.Any())
        {
            Console.WriteLine(
                $"Ticket {id} was not found or has no history.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"History for {id}:");

        foreach (var entry in history)
        {
            var type = entry.GetType();

            var timestamp =
                type.GetProperty("Timestamp")?.GetValue(entry);

            var eventName =
                type.GetProperty("Event")?.GetValue(entry);

            var details =
                type.GetProperty("Details")?.GetValue(entry);

            Console.WriteLine(
                $"{timestamp} | {eventName} | {details}");
        }
    }

    static void LoadSampleData()
    {
        var samplePath = Path.Combine(
            Environment.CurrentDirectory,
            "TicketingSystem",
            "sample_tickets.csv");

        if (!File.Exists(samplePath))
        {
            samplePath = Path.Combine(
                Environment.CurrentDirectory,
                "sample_tickets.csv");
        }

        if (!File.Exists(samplePath))
        {
            Console.WriteLine();
            Console.WriteLine(
                $"Sample file could not be found: {samplePath}");
            return;
        }

        var result = _service.ImportTicketsFromCsv(samplePath);

        Console.WriteLine();
        Console.WriteLine("SAMPLE DATA");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Loaded records:  {result.ValidRecords}");
        Console.WriteLine($"Invalid records: {result.InvalidRecords}");

        if (result.Errors.Any())
        {
            Console.WriteLine();

            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error}");
            }
        }
    }
}