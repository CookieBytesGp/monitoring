using App.Services;
using App.Hubs;
using App.Models.Auth;
using App.Services;
using App.Services.Interfaces;
using App.Services.BackgroundServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using App.Data;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("App")));

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ToolService>();
builder.Services.AddHttpClient<PageService>();
builder.Services.AddControllersWithViews();

// Add these services before building the app
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Register services
builder.Services.AddScoped<IMonitorService, MonitorService>();
builder.Services.AddScoped<ICameraService, CameraService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<IMotionDetectionService, MotionDetectionService>();
builder.Services.AddScoped<IMotionAnalyticsService, MotionAnalyticsService>();
// Register background services
builder.Services.AddHostedService<MonitorHealthCheckService>();
builder.Services.AddHostedService<CameraHealthCheckService>();

// Add SignalR
builder.Services.AddSignalR();

// Configure cookie policy
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hubs
app.MapHub<MonitorHub>("/monitorHub");
app.MapHub<CameraHub>("/cameraHub");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



// Add this section right after app.Build() and before app.Run()
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        context.Database.Migrate();

        // Create Admin Role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create Admin User if it doesn't exist
        var adminEmail = "admin@yoursite.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                CreatedReportTemplates = string.Empty,
                ModifiedReportTemplates = string.Empty
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                logger.LogInformation("Admin user created successfully");
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Admin role assigned successfully");
                }
                else
                {
                    logger.LogError("Failed to assign admin role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogError("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists");
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Admin role assigned to existing user");
                }
                else
                {
                    logger.LogError("Failed to assign admin role to existing user: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        // Call your existing seed method if needed
        await SeedData.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}


app.Run();
