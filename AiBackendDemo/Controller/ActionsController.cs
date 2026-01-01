using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AiBackendDemo.Repositories;

namespace AiBackendDemo.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActionsController : ControllerBase
    {
        private readonly IActionsRepository _actionsRepository;
        public ActionsController(IActionsRepository actionsRepository)
        {
            _actionsRepository = actionsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var actions = await _actionsRepository.GetAllActionsAsync();
            return Ok(actions);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string text)
        {
            var actions = await _actionsRepository.SearchActionsAsync(text);
            return Ok(actions);
        }
    }
}
