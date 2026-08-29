using MediatR;
using Microsoft.AspNetCore.Mvc;
using Underdog.API.Contracts;
using Underdog.Application.Intents.ProcessIntent;

namespace Underdog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IntentController : ControllerBase
{
    private readonly IMediator _mediator;

    public IntentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType<ProcessIntentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessAsync([FromBody] ProcessIntentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserPrompt))
        {
            return BadRequest("Il campo UserPrompt e obbligatorio.");
        }

        var result = await _mediator.Send(new ProcessIntentQuery(request.UserPrompt), cancellationToken);

        var response = new ProcessIntentResponse
        {
            ParsedObject = new
            {
                action = result.ParsedObject.Action,
                entity = result.ParsedObject.Subject
            },
            HumanPhrase = result.HumanPhrase,
            FromCache = result.FromCache
        };

        return Ok(response);
    }
}
