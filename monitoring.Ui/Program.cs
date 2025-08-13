using Monitoring.Ui.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Add HttpClient
builder.Services.AddHttpClient();

// Add API Services
builder.Services.AddScoped<ICameraApiService, CameraApiService>();

// Register SignalR adapters
//builder.Services.AddScoped<Monitoring.Application.Interfaces.Realtime.ICameraNotifications, App.Hubs.Adapters.SignalRCameraNotifications>();
//builder.Services.AddScoped<Monitoring.Application.Interfaces.Realtime.IMonitorNotifications, App.Hubs.Adapters.SignalRMonitorNotifications>();

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

// SignalR hubs
app.MapHub<App.Hubs.CameraHub>("/hubs/camera");
app.MapHub<App.Hubs.MonitorHub>("/hubs/monitor");

app.Run();
