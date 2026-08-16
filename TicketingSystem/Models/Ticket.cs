namespace TicketingSystem.Models;

/// represents a support ticket in the system.
public class Ticket
{
    public string Id { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
    public string AssignedTo { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsValid { get; set; } = true;
    public string ValidationError { get; set; } = string.Empty;

    // calculated properties
    public double ResponseTimeHours => FirstResponseAt.HasValue
        ? (FirstResponseAt.Value - CreatedAt).TotalHours
        : 0;

    public double ResolutionTimeHours => ResolvedAt.HasValue
        ? (ResolvedAt.Value - CreatedAt).TotalHours
        : 0;

    public bool IsSlaMet => ResolvedAt.HasValue && Priority.ToLower() switch
    {
        "critical" => ResolutionTimeHours <= 4,
        "high" => ResolutionTimeHours <= 8,
        "medium" => ResolutionTimeHours <= 24,
        "low" => ResolutionTimeHours <= 48,
        _ => ResolutionTimeHours <= 24
    };
}