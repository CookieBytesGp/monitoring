using Domain.SharedKernel;
using Domain.Aggregates.Page.ValueObjects;
using Monitoring.Application.DTOs.Page;

namespace DTOs.Pagebuilder
{
    public class PageDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // فیلدهای جدید
        public string Status { get; set; }
        public string ThumbnailUrl { get; set; }
        
        // اندازه نمایشگر
        public int DisplayWidth { get; set; }
        public int DisplayHeight { get; set; }
        
        // صدای پس‌زمینه (اختیاری)
        public AssetDTO BackgroundAsset { get; set; }
        
        public List<BaseElementDTO> Elements { get; set; } = new List<BaseElementDTO>();
    }
}
