 using System;
using System.ComponentModel.DataAnnotations;

namespace App.Models.FilterModels
{
    public class TemplateFilterModel
    {
        public string SearchTerm { get; set; }
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
        public bool? IsDefault { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public static class SortOptions
        {
            public const string Name = "name";
            public const string CreatedAt = "created";
            public const string LastModified = "modified";
        }
    }
}