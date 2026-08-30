using Underdog.Application.Abstractions;
using Underdog.Application.Configuration;
using Underdog.Application.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Underdog.Application.Services;

public sealed class IntentProcessingService(
    IIntentCacheService cacheService,
    IModelService modelService,
    IIntentObservabilityRepository intentObservabilityRepository,
    IOptions<AiModelOptions> modelOptions) : IIntentProcessingService
{
    private readonly IIntentCacheService _cacheService = cacheService;
    private readonly IModelService _modelService = modelService;
    private readonly IIntentObservabilityRepository _intentObservabilityRepository = intentObservabilityRepository;
    private readonly AiModelOptions _modelOptions = modelOptions.Value;

    public async Task<IntentResult> ProcessAsync(string userPrompt, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString("N");

        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            throw new ArgumentException("Il prompt utente non puo essere vuoto.", nameof(userPrompt));
        }

        var (normalizedPrompt, hash) = TextNormalizer.CleanAndHash(userPrompt);
        try
        {
            //var modelPath = ModelFingerprintService.ResolveAbsolutePath(_modelOptions);
           // var modelFileHash = ModelFingerprintService.ComputeSha256(modelPath);
            // await _intentObservabilityRepository.UpsertModelVersionAsync(
            //     _modelOptions.ModelVersion,
            //     modelPath,
            //     modelFileHash,
            //     cancellationToken);

            var cacheEntry = await _cacheService.GetByHashAsync(hash, cancellationToken);

            if (cacheEntry is not null)
            {
                await _cacheService.IncrementHitAsync(cacheEntry.Id, cancellationToken);
                var cachedParsed = JsonIntentParser.Parse(cacheEntry.RawJsonResponse);
                var cachedResult = new IntentResult
                {
                    ParsedObject = cachedParsed,
                    HumanPhrase = cacheEntry.HumanPhrase,
                    FromCache = true
                };

                stopwatch.Stop();
                // await _intentObservabilityRepository.SaveAuditAsync(
                //     correlationId,
                //     userPrompt,
                //     hash,
                //     cacheEntry.RawJsonResponse,
                //     cachedResult.HumanPhrase,
                //     true,
                //     "Success",
                //     null,
                //     cancellationToken);

                // await _intentObservabilityRepository.SaveMetricAsync(
                //     correlationId,
                //     (int)stopwatch.ElapsedMilliseconds,
                //     true,
                //     true,
                //     null,
                //     cancellationToken);

                return cachedResult;
            }

            var rawJson = await _modelService.InferRawJsonAsync(normalizedPrompt, cancellationToken);
            var parsed = JsonIntentParser.Parse(rawJson);

            await _cacheService.SaveAsync(userPrompt, normalizedPrompt, hash, parsed, rawJson, cancellationToken);

            var result = new IntentResult
            {
                ParsedObject = parsed,
                HumanPhrase = parsed.HumanPhrase,
                FromCache = false
            };

            stopwatch.Stop();
            // await _intentObservabilityRepository.SaveAuditAsync(
            //     correlationId,
            //     userPrompt,
            //     hash,
            //     rawJson,
            //     result.HumanPhrase,
            //     false,
            //     "Success",
            //     null,
            //     cancellationToken);

            // await _intentObservabilityRepository.SaveMetricAsync(
            //     correlationId,
            //     (int)stopwatch.ElapsedMilliseconds,
            //     false,
            //     true,
            //     null,
            //     cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            // await _intentObservabilityRepository.SaveAuditAsync(
            //     correlationId,
            //     userPrompt,
            //     hash,
            //     string.Empty,
            //     string.Empty,
            //     false,
            //     "Error",
            //     ex.Message,
            //     cancellationToken);

            // await _intentObservabilityRepository.SaveMetricAsync(
            //     correlationId,
            //     (int)stopwatch.ElapsedMilliseconds,
            //     false,
            //     false,
            //     ex.GetType().Name,
            //     cancellationToken);

            throw;
        }
    }
}
