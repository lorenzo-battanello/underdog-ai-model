using MediatR;
using Underdog.Application.Abstractions;
using Underdog.Application.Models;

namespace Underdog.Application.Intents.ProcessIntent;

public sealed class ProcessIntentQueryHandler : IRequestHandler<ProcessIntentQuery, IntentResult>
{
    private readonly IIntentProcessingService _intentProcessingService;

    public ProcessIntentQueryHandler(IIntentProcessingService intentProcessingService)
    {
        _intentProcessingService = intentProcessingService;
    }

    public async Task<IntentResult> Handle(ProcessIntentQuery request, CancellationToken cancellationToken)
    {
        return await _intentProcessingService.ProcessAsync(request.UserPrompt, cancellationToken);
    }
}
