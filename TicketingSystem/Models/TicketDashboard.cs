namespace TicketingSystem.Models;

/// dashboard summary data for support analytics.
public class TicketDashboard
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public double AverageResponseTimeHours { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double SlaCompliancePercentage { get; set; }
    public Dictionary<string, int> TicketsByPriority { get; set; } = new();
    public Dictionary<string, int> TicketsByCategory { get; set; } = new();
    public Dictionary<string, int> TicketsByChannel { get; set; } = new();
}