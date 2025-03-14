using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models
{
    public class ReportExportHistory
    {
        public int Id { get; set; }

        [Required]
        public int ReportTemplateId { get; set; }

        [ForeignKey("ReportTemplateId")]
        public virtual ReportTemplate ReportTemplate { get; set; }

        public DateTime ExportedAt { get; set; }
        
        [Required]
        public string Format { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int RecordsCount { get; set; }
        public long FileSizeBytes { get; set; }

        public bool WasScheduled { get; set; }
        public bool EmailSent { get; set; }
        
        [StringLength(500)]
        public string EmailRecipients { get; set; }

        public string Status { get; set; } // Success, Failed
        
        [StringLength(1000)]
        public string ErrorMessage { get; set; }

        [StringLength(255)]
        public string FileName { get; set; }

        public string GeneratedBy { get; set; } // User who generated the report

        public string ExportedBy { get; set; }
        public string FilePath { get; set; }
    }
}