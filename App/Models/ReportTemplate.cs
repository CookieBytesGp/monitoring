using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models
{
    public class ReportTemplate
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        [Display(Name = "Template Name")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Format is required")]
        [RegularExpression("^(CSV|JSON|EXCEL)$", ErrorMessage = "Invalid format. Allowed values are: CSV, JSON, EXCEL")]
        [Display(Name = "Report Format")]
        public string Format { get; set; }

        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Last Generated")]
        public DateTime? LastGeneratedAt { get; set; }

        // Report Content Settings
        [Display(Name = "Include Images")]
        public bool IncludeImages { get; set; }

        [Display(Name = "Include Motion Statistics")]
        public bool IncludeMotionStats { get; set; }

        [Display(Name = "Include Image Analysis")]
        public bool IncludeImageAnalysis { get; set; }

        [Display(Name = "Include Processing History")]
        public bool IncludeProcessingHistory { get; set; }

        // Time Range Settings
        [Required(ErrorMessage = "Time range type is required")]
        [RegularExpression("^(Daily|Weekly|Monthly|Custom)$", ErrorMessage = "Invalid time range type")]
        [Display(Name = "Time Range")]
        public string TimeRangeType { get; set; }

        [Range(1, 365, ErrorMessage = "Custom days must be between 1 and 365")]
        [Display(Name = "Number of Days")]
        public int? CustomDays { get; set; }

        // Filter Settings
        [Display(Name = "Camera Filter")]
        public string CameraFilter { get; set; }  // Stored as comma-separated string

        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        [Display(Name = "Minimum Motion Percentage")]
        public float? MinMotionPercentage { get; set; }

        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        [Display(Name = "Maximum Motion Percentage")]
        public float? MaxMotionPercentage { get; set; }

        [Display(Name = "Only Acknowledged Events")]
        public bool OnlyAcknowledged { get; set; }

        // Grouping Settings
        [Display(Name = "Group by Camera")]
        public bool GroupByCamera { get; set; }

        [Display(Name = "Group by Date")]
        public bool GroupByDate { get; set; }

        // Scheduling Settings
        [Display(Name = "Enable Scheduling")]
        public bool IsScheduled { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Schedule (Cron Expression)")]
        public string Schedule { get; set; }
        
        [StringLength(500)]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})(,\s*([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}))*$",
            ErrorMessage = "Please enter valid email addresses separated by commas")]
        [Display(Name = "Email Recipients")]
        public string EmailRecipients { get; set; }

        public virtual ICollection<ReportExportHistory> ReportExportHistory { get; set; }

        [NotMapped]
        public List<int> ParsedCameraFilter
        {
            get
            {
                if (string.IsNullOrEmpty(CameraFilter))
                    return new List<int>();

                var result = new List<int>();
                foreach (var item in CameraFilter.Split(','))
                {
                    if (int.TryParse(item.Trim(), out int id))
                        result.Add(id);
                }
                return result;
            }
        }

        public ReportTemplate()
        {
            CreatedAt = DateTime.UtcNow;
            Format = "CSV";
            IncludeMotionStats = true;
            TimeRangeType = "Daily";
            GroupByCamera = true;
            GroupByDate = true;
            IsScheduled = false;
            ReportExportHistory = new List<ReportExportHistory>();
        }
    }
}