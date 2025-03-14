 using System;

public class ProcessingTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Enhancement Settings
    public float? Brightness { get; set; }
    public float? Contrast { get; set; }
    public float? Sharpness { get; set; }

    // Resize Settings
    public int? Width { get; set; }
    public int? Height { get; set; }
    public bool MaintainAspectRatio { get; set; }

    // Rotation Settings
    public int? RotationDegrees { get; set; }

    public ProcessingTemplate()
    {
        CreatedAt = DateTime.UtcNow;
        MaintainAspectRatio = true;
    }
}