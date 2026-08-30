using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Underdog.Application.Abstractions;

namespace Underdog.Application.Services;

public sealed class CachedGgufModelService(IModelService innerService, IMemoryCache cache) : IModelService
{
    private readonly IModelService _innerService = innerService;
    private readonly IMemoryCache _cache = cache;

    public async Task<string> InferRawJsonAsync(string userSentence, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userSentence))
            return string.Empty;

        var cleanedSentence = userSentence.Trim().ToLowerInvariant();
        var cacheKey = $"intent_cache_{ComputeSha256Hash(cleanedSentence)}";

        if (_cache.TryGetValue(cacheKey, out string? cachedJson) && !string.IsNullOrEmpty(cachedJson))
        {
            return cachedJson;
        }

        var jsonResult = await _innerService.InferRawJsonAsync(userSentence, cancellationToken);

        if (!string.IsNullOrWhiteSpace(jsonResult) && jsonResult.StartsWith("{"))
        {
            _cache.Set(cacheKey, jsonResult, TimeSpan.FromDays(7));
        }

        return jsonResult;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes);
    }
}