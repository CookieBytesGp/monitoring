using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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

// ... existing code ... 