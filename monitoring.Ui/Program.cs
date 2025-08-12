using Monitoring.Application.Mappings;
using Monitoring.Application.Services.Page;
using Monitoring.Application.Services.Camera;
using Monitoring.Application.Interfaces.Camera;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Application Services
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<ICameraService, CameraService>();

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
