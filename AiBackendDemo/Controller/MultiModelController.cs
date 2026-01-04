using AiBackendDemo.DTOs;
using AiBackendDemo.Queries;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AiBackendDemo.Controller;

[ApiController]
[Route("api/[controller]")]
public class MultiModelController : ControllerBase
{
    private readonly IMultiModelService _multiModelService;

    public MultiModelController(IMultiModelService multiModelService)
    {
        _multiModelService = multiModelService;
    }

    [HttpPost("analyze-screenshot")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> AnalyzeScreenshot([FromForm] AnalyzeScreenshotRequest request, CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        await request.FileUpload.CopyToAsync(memoryStream, ct);
        var imageBytes = memoryStream.ToArray();
        var analysisResult = await _multiModelService.AnalyzeScreenshotAsync(imageBytes, ct);
        return Ok(analysisResult);
    }
    
    [HttpPost("extract-text")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> ExtractTextFromImage([FromForm] ExtractTextRequest request, CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        await request.FileUpload.CopyToAsync(memoryStream, ct);
        var imageBytes = memoryStream.ToArray();
        var extractedText = await _multiModelService.ExtractTextFromImageAsync(imageBytes, ct);
        return Ok(extractedText);
    }
    
    [HttpPost("analyze-image-issue")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> AnalyzeImageForIssue([FromForm] AnalyzeScreenshotRequest request, CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        await request.FileUpload.CopyToAsync(memoryStream, ct);
        var imageBytes = memoryStream.ToArray();
        var issueAnalysis = await _multiModelService.AnalyzeImageForIssueAsync(imageBytes, ct);
        return Ok(issueAnalysis);
    }
    
    [HttpPost("transcribe-audio")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> TranscribeAudio([FromForm] TranscribeAudioRequest request, CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        await request.AudioFile.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;
        var transcription = await _multiModelService.TranscribeAudioAsync(memoryStream, request.AudioFile.FileName, ct);
        return Ok(transcription);
    }
}