using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/classify")]
public class ClassificationController : ControllerBase
{
    private static readonly string[] Labels =
    {
        "SAFETY",
        "SECURITY",
        "OPERATIONS",
        "COMPLIANCE",
        "OTHER"
    };

    private readonly IAiTextClassifier _classifier;

    public ClassificationController(IAiTextClassifier classifier)
    {
        _classifier = classifier;
    }

    [HttpPost]
    public async Task<IActionResult> Classify([FromBody] string text, CancellationToken ct)
    {
        var result = await _classifier.ClassifyAsync(text, Labels, ct);
        return Ok(result);
    }
}
