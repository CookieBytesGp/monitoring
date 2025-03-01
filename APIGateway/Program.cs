using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddOcelot();

// Add logging
builder.Logging.AddConsole();
// Load Ocelot configuration file
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// Add Ocelot middleware AFTER MVC routing
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

await app.UseOcelot(); // Place Ocelot middleware after MVC routing
app.Run();
