namespace TicketingSystem.Models;

/// result of importing tickets from a csv file.
public class TicketImportResult
{
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public int InvalidRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Ticket> ValidTickets { get; set; } = new();
    public List<Ticket> InvalidTickets { get; set; } = new();
}