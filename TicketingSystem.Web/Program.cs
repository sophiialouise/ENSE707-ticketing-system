using TicketingSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// keep one ticket service instance while the web app is running
builder.Services.AddSingleton<TicketService>();

var app = builder.Build();

// load the sample ticket data for the initial prototype
var samplePath = Path.GetFullPath(
    Path.Combine(
        app.Environment.ContentRootPath,
        "..",
        "TicketingSystem",
        "sample_tickets.csv"));

if (File.Exists(samplePath))
{
    var ticketService = app.Services.GetRequiredService<TicketService>();

    if (!ticketService.GetTickets().Any())
    {
        ticketService.ImportTicketsFromCsv(samplePath);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();