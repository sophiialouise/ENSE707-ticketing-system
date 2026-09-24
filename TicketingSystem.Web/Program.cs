using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using TicketingSystem.Services;
using TicketingSystem.Web.Models;
using TicketingSystem.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// register mvc controllers and razor views
builder.Services.AddControllersWithViews();

// keep one ticket service instance while the web app is running
builder.Services.AddSingleton<TicketService>();

// register local authentication services
builder.Services.AddSingleton<
    IPasswordHasher<UserAccount>,
    PasswordHasher<UserAccount>>();

builder.Services.AddSingleton<UserAuthenticationService>();

// use cookie authentication for the local prototype
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(30);

        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // the prototype currently runs locally over HTTP
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// authentication must run before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// use the dashboard as the default page when the application starts
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();