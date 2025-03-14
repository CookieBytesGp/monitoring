using Microsoft.AspNetCore.Identity;
using App.Models.Auth;
using App.Models.Camera;
using App.Models;

namespace App.Data
{
    public static class SeedData
    {
        public static async Task Initialize(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure we have the database
            context.Database.EnsureCreated();

            // Seed Roles
            string[] roles = { "Administrator", "Operator", "Viewer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin User
            if (!context.Users.Any())
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
            }

            // Seed Email Templates
            if (!context.EmailTemplates.Any())
            {
                context.EmailTemplates.AddRange(
                    new EmailTemplate
                    {
                        Name = "Motion Detection Alert",
                        Subject = "Motion Detected - {CameraName}",
                        Body = "Motion was detected on camera {CameraName} at {Location} at {Timestamp}.",
                        Description = "Default template for motion detection alerts",
                        IsDefault = true
                    },
                    new EmailTemplate
                    {
                        Name = "System Alert",
                        Subject = "System Alert - {AlertType}",
                        Body = "A system alert has occurred: {Message}",
                        Description = "Default template for system alerts",
                        IsDefault = true
                    }
                );
            }

            // Seed Report Templates
            if (!context.ReportTemplates.Any())
            {
                context.ReportTemplates.Add(new ReportTemplate
                {
                    Name = "Daily Motion Summary",
                    Description = "Daily summary of motion events across all cameras",
                    Format = "CSV",
                    TimeRangeType = "Daily",
                    IncludeMotionStats = true,
                    GroupByCamera = true,
                    GroupByDate = true
                });
            }

            // Save changes
            await context.SaveChangesAsync();
        }
    }
}