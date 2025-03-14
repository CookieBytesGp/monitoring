using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using App.Models.Theme;

namespace App.Models.Auth
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties for audit
        public string CreatedReportTemplates { get; set; }
        public string ModifiedReportTemplates { get; set; }

        private string _theme = ThemeSettings.Light;

        [StringLength(20)]
        public string Theme
        {
            get => _theme;
            set => _theme = ThemeSettings.IsValidTheme(value) ? value.ToLower() : ThemeSettings.GetDefaultTheme();
        }

        public ApplicationUser()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
                    return UserName;
                
                return $"{FirstName} {LastName}".Trim();
            }
        }
    }
}