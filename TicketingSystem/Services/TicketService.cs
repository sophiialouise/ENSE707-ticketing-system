using TicketingSystem.Models;
using System.Globalization;
using CsvHelper;

namespace TicketingSystem.Services;

/// core service for managing tickets.
public class TicketService
{
    private readonly List<Ticket> _tickets = new();
    private int _nextId = 1;

    /// imports tickets from a csv file.
    public TicketImportResult ImportTicketsFromCsv(string filePath)
    {
        var result = new TicketImportResult();

        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<dynamic>().ToList();
            result.TotalRecords = records.Count;

            foreach (var record in records)
            {
                var ticket = MapRecordToTicket(record);

                if (ValidateTicket(ticket, out string error))
                {
                    ticket.IsValid = true;
                    ticket.Id = $"TICKET-{_nextId++:D4}";
                    result.ValidTickets.Add(ticket);
                }
                else
                {
                    ticket.IsValid = false;
                    ticket.ValidationError = error;
                    result.InvalidTickets.Add(ticket);
                    result.Errors.Add(error);
                }
            }

            result.ValidRecords = result.ValidTickets.Count;
            result.InvalidRecords = result.InvalidTickets.Count;

            // add valid tickets to the system
            _tickets.AddRange(result.ValidTickets);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Import failed: {ex.Message}");
        }

        return result;
    }

    /// maps a csv record to a ticket object.
    private Ticket MapRecordToTicket(dynamic record)
    {
        var priority = GetValue(record, "Priority");
        var status = GetValue(record, "Status");

        return new Ticket
        {
            CustomerName = GetValue(record, "CustomerName"),
            CustomerEmail = GetValue(record, "CustomerEmail"),
            Category = GetValue(record, "Category"),
            Priority = string.IsNullOrWhiteSpace(priority) ? "Medium" : priority,
            Status = string.IsNullOrWhiteSpace(status) ? "Open" : status,
            AssignedTo = GetValue(record, "AssignedTo"),
            Channel = GetValue(record, "Channel"),
            Description = GetValue(record, "Description"),
            CreatedAt = ParseDate(GetValue(record, "CreatedAt"), DateTime.Now),
            ResolvedAt = ParseNullableDate(GetValue(record, "ResolvedAt"))
        };
    }

    /// gets a value from a dynamic csv record.
    private string GetValue(dynamic record, string field)
    {
        var values = record as IDictionary<string, object>;

        if (values != null && values.TryGetValue(field, out var value))
        {
            return value?.ToString() ?? string.Empty;
        }

        return string.Empty;
    }

    /// parses a required date value.
    private DateTime ParseDate(string value, DateTime defaultValue)
    {
        return DateTime.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    /// parses an optional date value.
    private DateTime? ParseNullableDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParse(value, out var result)
            ? result
            : null;
    }

    /// <summary>
    /// validates a ticket record.
    /// </summary>
    private bool ValidateTicket(Ticket ticket, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(ticket.CustomerName))
        {
            error = "Customer name is required.";
        }
        else if (string.IsNullOrWhiteSpace(ticket.CustomerEmail))
        {
            error = "Customer email is required.";
        }
        else if (!ticket.CustomerEmail.Contains("@"))
        {
            error = "Invalid email format.";
        }
        else if (string.IsNullOrWhiteSpace(ticket.Category))
        {
            error = "Category is required.";
        }
        else if (ticket.ResolvedAt.HasValue &&
                 ticket.ResolvedAt.Value < ticket.CreatedAt)
        {
            error = "Resolved time cannot be before created time.";
        }

        return string.IsNullOrEmpty(error);
    }

    /// returns tickets with optional filtering.
    public List<Ticket> GetTickets(TicketFilter? filter = null)
    {
        var query = _tickets.AsQueryable();

        if (filter != null)
        {
            if (filter.StartDate.HasValue)
            {
                query = query.Where(
                    t => t.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(
                    t => t.CreatedAt <= filter.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                query = query.Where(
                    t => t.Category.Equals(
                        filter.Category,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Priority))
            {
                query = query.Where(
                    t => t.Priority.Equals(
                        filter.Priority,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(
                    t => t.Status.Equals(
                        filter.Status,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.AssignedTo))
            {
                query = query.Where(
                    t => t.AssignedTo.Equals(
                        filter.AssignedTo,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Channel))
            {
                query = query.Where(
                    t => t.Channel.Equals(
                        filter.Channel,
                        StringComparison.OrdinalIgnoreCase));
            }
        }

        return query.ToList();
    }

    /// returns dashboard summary data.
    public TicketDashboard GetDashboard(TicketFilter? filter = null)
    {
        var tickets = GetTickets(filter);

        var resolvedTickets = tickets
            .Where(t => t.ResolvedAt.HasValue)
            .ToList();

        var respondedTickets = tickets
            .Where(t => t.FirstResponseAt.HasValue)
            .ToList();

        return new TicketDashboard
        {
            TotalTickets = tickets.Count,

            OpenTickets = tickets.Count(
                t => t.Status.Equals(
                    "Open",
                    StringComparison.OrdinalIgnoreCase)),

            InProgressTickets = tickets.Count(
                t => t.Status.Equals(
                    "In Progress",
                    StringComparison.OrdinalIgnoreCase)),

            ResolvedTickets = tickets.Count(
                t => t.Status.Equals(
                         "Resolved",
                         StringComparison.OrdinalIgnoreCase) ||
                     t.Status.Equals(
                         "Closed",
                         StringComparison.OrdinalIgnoreCase)),

            AverageResponseTimeHours = respondedTickets.Any()
                ? respondedTickets.Average(t => t.ResponseTimeHours)
                : 0,

            AverageResolutionTimeHours = resolvedTickets.Any()
                ? resolvedTickets.Average(t => t.ResolutionTimeHours)
                : 0,

            SlaCompliancePercentage = resolvedTickets.Any()
                ? (double)resolvedTickets.Count(t => t.IsSlaMet)
                    / resolvedTickets.Count * 100
                : 0,

            TicketsByPriority = tickets
                .GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => g.Count()),

            TicketsByCategory = tickets
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Count()),

            TicketsByChannel = tickets
                .GroupBy(t => t.Channel)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// returns a ticket using its id.
    public Ticket? GetTicketById(string id)
    {
        return _tickets.FirstOrDefault(
            t => t.Id.Equals(
                id,
                StringComparison.OrdinalIgnoreCase));
    }

    /// updates the status of a ticket.
    public bool UpdateTicketStatus(string id, string newStatus)
    {
        var ticket = GetTicketById(id);

        if (ticket == null)
        {
            return false;
        }

        var validStatuses = new[]
        {
            "Open",
            "In Progress",
            "Resolved",
            "Closed"
        };

        var matchedStatus = validStatuses.FirstOrDefault(
            status => status.Equals(
                newStatus,
                StringComparison.OrdinalIgnoreCase));

        if (matchedStatus == null)
        {
            return false;
        }

        ticket.Status = matchedStatus;

        if (matchedStatus == "In Progress" &&
            !ticket.FirstResponseAt.HasValue)
        {
            ticket.FirstResponseAt = DateTime.Now;
        }

        if (matchedStatus == "Resolved" ||
            matchedStatus == "Closed")
        {
            ticket.ResolvedAt = DateTime.Now;
        }

        return true;
    }

    /// returns an sla compliance report.
    public object GetSlaReport(TicketFilter? filter = null)
    {
        var tickets = GetTickets(filter);

        var resolved = tickets
            .Where(t => t.ResolvedAt.HasValue)
            .ToList();

        return new
        {
            TotalResolved = resolved.Count,

            SlaMet = resolved.Count(t => t.IsSlaMet),

            SlaMissed = resolved.Count(t => !t.IsSlaMet),

            CompliancePercentage = resolved.Any()
                ? Math.Round(
                    (double)resolved.Count(t => t.IsSlaMet)
                    / resolved.Count * 100,
                    2)
                : 0,

            ByPriority = resolved
                .GroupBy(t => t.Priority)
                .Select(g => new
                {
                    Priority = g.Key,
                    Total = g.Count(),
                    SlaMet = g.Count(t => t.IsSlaMet),
                    Compliance = Math.Round(
                        (double)g.Count(t => t.IsSlaMet)
                        / g.Count() * 100,
                        2)
                })
                .ToList()
        };
    }

    /// returns the available history for a ticket.
    public List<object> GetTicketHistory(string ticketId)
    {
        var ticket = GetTicketById(ticketId);

        if (ticket == null)
        {
            return new List<object>();
        }

        var history = new List<object>
        {
            new
            {
                Timestamp = ticket.CreatedAt,
                Event = "Ticket created",
                Details = $"Customer: {ticket.CustomerName}"
            }
        };

        if (ticket.FirstResponseAt.HasValue)
        {
            history.Add(new
            {
                Timestamp = ticket.FirstResponseAt.Value,
                Event = "First response",
                Details = "Staff response recorded"
            });
        }

        if (ticket.ResolvedAt.HasValue)
        {
            history.Add(new
            {
                Timestamp = ticket.ResolvedAt.Value,
                Event = "Ticket resolved",
                Details =
                    $"Resolution time: {ticket.ResolutionTimeHours:F2} hours"
            });
        }

        return history;
    }
}