using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AiBackendDemo.Repositories;
using AiBackendDemo.Queries;

namespace AiBackendDemo.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActionsController : ControllerBase
    {
        private readonly IActionAgentQueryService _actionAgentQueryService;
        public ActionsController(IActionAgentQueryService actionAgentQueryService)
        {
            _actionAgentQueryService = actionAgentQueryService;
        }

        [HttpGet("semantic-search")]
        public async Task<IActionResult> SemanticSearch([FromQuery] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("Search text is required.");

            var results = await _actionAgentQueryService.SemanticSearchAsync(text);
            return Ok(results);
        }

        [HttpGet("{actionId}/summary")]
        public async Task<IActionResult> GetActionSummary([FromRoute] int actionId)
        {
            var summary = await _actionAgentQueryService.GetActionSummaryAsync(actionId);
            return Ok(summary);
        }

        [HttpGet("rag-search")]
        public async Task<IActionResult> RagSearch([FromQuery] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("Search text is required.");
            var results = await _actionAgentQueryService.RagSearchAsync(text);
            return Ok(results);

        }
    }
}