using System.ComponentModel.DataAnnotations;

namespace AiBackendDemo.DTOs;

public class AnalyzeScreenshotRequest
{
    [Required]
    public IFormFile FileUpload { get; set; } = default!;
}


public class ExtractTextRequest
{
    [Required]
    public IFormFile FileUpload { get; set; } = default!;
}