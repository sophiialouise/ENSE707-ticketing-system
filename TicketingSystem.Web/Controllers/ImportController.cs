using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Services;

namespace TicketingSystem.Web.Controllers;

public class ImportController : Controller
{
    private readonly TicketService _ticketService;

    public ImportController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.ErrorMessage = "Please select a CSV file.";
            return View();
        }

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".csv",
                StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.ErrorMessage = "Only CSV files can be imported.";
            return View();
        }

        var tempPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.csv");

        try
        {
            await using (var stream = System.IO.File.Create(tempPath))
            {
                await file.CopyToAsync(stream);
            }

            var result =
                _ticketService.ImportTicketsFromCsv(tempPath);

            return View(result);
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
        }
    }
}