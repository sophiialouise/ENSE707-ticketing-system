namespace TicketingSystem.Models;

/// filter criteria for ticket queries.
public class TicketFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? AssignedTo { get; set; }
    public string? Channel { get; set; }
}