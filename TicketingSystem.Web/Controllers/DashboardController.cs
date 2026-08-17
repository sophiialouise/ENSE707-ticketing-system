using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Services;

namespace TicketingSystem.Web.Controllers;

public class DashboardController : Controller
{
    private readonly TicketService _ticketService;

    public DashboardController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public IActionResult Index()
    {
        var dashboard = _ticketService.GetDashboard();

        return View(dashboard);
    }
}