using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using pawKitAppBlazorServer.Data;

// This line starts the process of building your web application.
// It sets up everything your app will need, using any instructions you give it when you start the app.
var builder = WebApplication.CreateBuilder(args);

// By default, ASP.NET Core configures logging to output to the Console (terminal/command prompt) and Debug (Visual Studio Output window).
// These logging destinations are set up automatically when you use WebApplication.CreateBuilder(args).
// You can customize or add more logging providers (like files, cloud, or databases) in appsettings.json or with builder.Logging.AddXyz().

// Here, you tell your app what special features and helpers it should use.
// These helpers are called "services" and they make your app smarter and easier to build.

// This service lets you use Razor Pages, which are a way to create web pages using simple templates.
builder.Services.AddRazorPages();

// This service enables Blazor Server, which lets you build interactive web pages that can update instantly without reloading.
builder.Services.AddServerSideBlazor();

// This service provides weather data to your app. By making it a "singleton," you ensure there's only one copy of it running, saving memory and keeping things organized.
builder.Services.AddSingleton<WeatherForecastService>();

// This line finishes setting up your app and gets it ready to run.
var app = builder.Build();

// Now you set up the "pipeline" for handling requests from people visiting your website.
// Think of this like a series of checkpoints that every visitor goes through.

// If your app is running in production (not just for testing), you want to be extra careful with errors and security.
if (!app.Environment.IsDevelopment())
{
    // If something goes wrong, send visitors to a friendly error page instead of showing a scary error message.
    app.UseExceptionHandler("/Error");
    // HSTS is a security feature that tells browsers to always use secure connections (HTTPS) for your site.
    // This helps protect your visitors from certain types of attacks.
    app.UseHsts();
}

// This makes sure everyone uses a secure connection (HTTPS) when visiting your site.
// If someone tries to use an insecure connection (HTTP), they'll be automatically redirected to the secure version.
app.UseHttpsRedirection();

// This lets your app serve files like images, stylesheets, and scripts directly to visitors.
// These files are stored in a folder called "wwwroot" and don't change often.
app.UseStaticFiles();

// This turns on routing, which is like a traffic controller for your website.
// Routing looks at the web address (URL) someone visits and decides which part of your app should respond.
// For example, if someone visits '/about', routing makes sure the About page is shown.
// Without routing, your app wouldn't know how to handle different requests from users.
app.UseRouting();

// This sets up a special connection called a "SignalR hub" for Blazor Server.
// In Blazor Server, the web pages you see in your browser are actually controlled by the server.
// When you click a button or type something, your actions are sent to the server, which decides what should happen next.
// The "hub" is like a telephone line that keeps your browser and the server talking to each other instantly.
// Thanks to this connection, your page can update right away—without needing to reload or refresh.
// Without this hub, Blazor Server apps wouldn't be able to provide live, interactive experiences.
app.MapBlazorHub();

// If someone visits a web address that doesn't match any of your static files or Razor pages,
// this sends them to the main Blazor entry page (_Host.cshtml).
// _Host.cshtml loads your Blazor app in the browser.
// Once loaded, Blazor takes over and looks at the address in the browser.
// If the address matches a Blazor page, it shows that page.
// If the address doesn't match any Blazor page, Blazor shows your custom "Not Found" message from App.razor.
// This setup is needed so Blazor can handle navigation and "not found" pages inside the app itself.
// In short: the server always sends _Host.cshtml for unknown URLs, and Blazor decides what to show next.
app.MapFallbackToPage("/_Host");

// This final line starts your web application and begins listening for visitors.
// Your site is now live and ready to handle requests!
app.Run();
