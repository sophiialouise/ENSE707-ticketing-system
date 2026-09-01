using TicketingSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// register mvc controllers and razor views
builder.Services.AddControllersWithViews();

// keep one ticket service instance while the web app is running
builder.Services.AddSingleton<TicketService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// use the dashboard as the default page when the application starts
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();