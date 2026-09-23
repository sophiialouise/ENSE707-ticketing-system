using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Models;
using TicketingSystem.Services;

namespace TicketingSystem.Web.Controllers;

// displays dashboard totals and quality metrics from the ticket service
public class DashboardController : Controller
{
    private readonly TicketService _ticketService;

    public DashboardController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public IActionResult Index(
        DateTime? startDate,
        DateTime? endDate,
        string? assignedTo,
        string? category,
        string? channel)
    {
        // build the dashboard filter from the selected values
        var filter = new TicketFilter
        {
            StartDate = startDate,
            EndDate = endDate,
            AssignedTo = assignedTo,
            Category = category,
            Channel = channel
        };

        // calculate dashboard values using only matching tickets
        var dashboard = _ticketService.GetDashboard(filter);

        // keep the selected filters visible after the page reloads
        ViewBag.StartDate =
            startDate?.ToString("yyyy-MM-dd");

        ViewBag.EndDate =
            endDate?.ToString("yyyy-MM-dd");

        ViewBag.AssignedTo = assignedTo;
        ViewBag.Category = category;
        ViewBag.Channel = channel;

        return View(dashboard);
    }
}