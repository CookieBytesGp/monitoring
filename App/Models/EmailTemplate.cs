 using System;
using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class EmailTemplate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        // Available placeholders for the template
        public static class Placeholders
        {
            public const string ReportName = "{{ReportName}}";
            public const string DateRange = "{{DateRange}}";
            public const string GeneratedDate = "{{GeneratedDate}}";
            public const string RecordsCount = "{{RecordsCount}}";
            public const string FileFormat = "{{FileFormat}}";
            public const string FileSize = "{{FileSize}}";
            public const string GeneratedBy = "{{GeneratedBy}}";
        }
    }
}