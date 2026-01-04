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
}