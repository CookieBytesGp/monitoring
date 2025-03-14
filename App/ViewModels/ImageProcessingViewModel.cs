 using System.ComponentModel.DataAnnotations;

namespace App.ViewModels
{
    public class ImageProcessingViewModel
    {
        public int EventId { get; set; }
        
        public string ImagePath { get; set; }
        
        [Range(-1, 1)]
        public float Brightness { get; set; }
        
        [Range(-1, 1)]
        public float Contrast { get; set; }
        
        [Range(0, 1)]
        public float Sharpness { get; set; }
        
        [Range(1, 10000)]
        public int Width { get; set; }
        
        [Range(1, 10000)]
        public int Height { get; set; }
        
        public bool MaintainAspectRatio { get; set; } = true;
        
        public string AnnotationText { get; set; }
        
        [Range(8, 72)]
        public float FontSize { get; set; } = 16;
        
        public string TextColor { get; set; } = "#FF0000";
        
        public float RotationDegrees { get; set; }
        
        public string ProcessedImagePath { get; set; }
    }

    public class ImageAnalysisResult
    {
        public float Brightness { get; set; }
        public float Contrast { get; set; }
        public bool IsBlurry { get; set; }
    }

    public class SaveChangesRequest
    {
        public int EventId { get; set; }
        public string ProcessedImagePath { get; set; }
    }
}