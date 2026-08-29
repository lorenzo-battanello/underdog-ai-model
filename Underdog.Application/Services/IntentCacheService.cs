using Underdog.Application.Abstractions;
using Underdog.Application.Models;

namespace Underdog.Application.Services;

public sealed class IntentCacheService : IIntentCacheService
{
    private readonly IIntentCacheRepository _intentCacheRepository;

    public IntentCacheService(IIntentCacheRepository intentCacheRepository)
    {
        _intentCacheRepository = intentCacheRepository;
    }

    public async Task<AiIntentCacheRow?> GetByHashAsync(string normalizedPromptHash, CancellationToken cancellationToken = default)
    {
        return await _intentCacheRepository.GetByHashAsync(normalizedPromptHash, cancellationToken);
    }

    public async Task IncrementHitAsync(ulong id, CancellationToken cancellationToken = default)
    {
        await _intentCacheRepository.IncrementHitAsync(id, cancellationToken);
    }

    public async Task SaveAsync(string originalPrompt, string normalizedPrompt, string normalizedPromptHash, ParsedIntent parsedIntent, string rawJsonResponse, CancellationToken cancellationToken = default)
    {
        await _intentCacheRepository.SaveAsync(
            originalPrompt,
            normalizedPrompt,
            normalizedPromptHash,
            parsedIntent,
            rawJsonResponse,
            cancellationToken);
    }
}
