using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Services;

namespace TicketingSystem.Web.Controllers;

// // displays dashboard totals and quality metrics from the ticket service
public class DashboardController : Controller
{
    private readonly TicketService _ticketService;

    public DashboardController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public IActionResult Index()
    {
        // get the latest dashboard values from the shared in-memory service
        var dashboard = _ticketService.GetDashboard();

        return View(dashboard);
    }
}