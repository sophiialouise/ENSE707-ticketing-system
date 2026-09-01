using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Models;
using TicketingSystem.Services;

namespace TicketingSystem.Web.Controllers;

// handles ticket filtering, details and status updates
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
        // build the filter from the values selected on the tickets page
        var filter = new TicketFilter
        {
            Category = category,
            Priority = priority,
            Status = status,
            AssignedTo = assignedTo,
            Channel = channel
        };

        var tickets = _ticketService.GetTickets(filter);

        // keep the selected values visible when the filtered page reloads
        ViewBag.Category = category;
        ViewBag.Priority = priority;
        ViewBag.Status = status;
        ViewBag.AssignedTo = assignedTo;
        ViewBag.Channel = channel;

        return View(tickets);
    }

    public IActionResult Details(string id)
    {
        var ticket = _ticketService.GetTicketById(id);

        if (ticket == null)
        {
            return NotFound();
        }

        // load the available ticket history for the details page
        ViewBag.History = _ticketService.GetTicketHistory(id);

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateStatus(string id, string newStatus)
    {
        // reject incomplete status update requests before calling the service
        if (string.IsNullOrWhiteSpace(id) ||
            string.IsNullOrWhiteSpace(newStatus))
        {
            TempData["ErrorMessage"] =
                "A ticket and status are required.";

            return RedirectToAction("Index");
        }

        var updated =
            _ticketService.UpdateTicketStatus(id, newStatus);

        if (!updated)
        {
            TempData["ErrorMessage"] =
                "The ticket status could not be updated.";
        }
        else
        {
            TempData["SuccessMessage"] =
                $"Ticket {id} was updated to {newStatus}.";
        }

        return RedirectToAction("Details", new { id });
    }
}