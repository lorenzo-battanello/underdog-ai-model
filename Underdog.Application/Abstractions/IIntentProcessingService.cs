using Underdog.Application.Models;

namespace Underdog.Application.Abstractions;

public interface IIntentProcessingService
{
    Task<IntentResult> ProcessAsync(string userPrompt, CancellationToken cancellationToken = default);
}
