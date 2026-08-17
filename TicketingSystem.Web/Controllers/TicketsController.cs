using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Models;
using TicketingSystem.Services;

namespace TicketingSystem.Web.Controllers;

public class TicketsController : Controller
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public IActionResult Index(
        string? category,
        string? priority,
        string? status,
        string? assignedTo,
        string? channel)
    {
        var filter = new TicketFilter
        {
            Category = category,
            Priority = priority,
            Status = status,
            AssignedTo = assignedTo,
            Channel = channel
        };

        var tickets = _ticketService.GetTickets(filter);

        ViewBag.Category = category;
        ViewBag.Priority = priority;
        ViewBag.Status = status;
        ViewBag.AssignedTo = assignedTo;
        ViewBag.Channel = channel;

        return View(tickets);
    }
}