using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Options;
using Underdog.Application.Abstractions;
using Underdog.Application.Configuration;

namespace Underdog.Application.Services;

public sealed class GgufModelService : IModelService, IDisposable
{
    private readonly AiModelOptions _options;
    private readonly LLamaWeights _modelWeights;
    private readonly LLamaContext _context;
    private readonly InteractiveExecutor _executor;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly string _stateFilePath;

    public GgufModelService(IOptions<AiModelOptions> options)
    {
        _options = options.Value;

        var modelPath = Path.GetFullPath(_options.ModelPath);
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"File modello non trovato: {modelPath}");
        }

        var modelParams = new ModelParams(modelPath)
        {
            ContextSize = _options.ContextSize > 0 ? _options.ContextSize : 512,
            GpuLayerCount = _options.GpuLayerCount,
            Threads = Math.Max(1, Environment.ProcessorCount - 1),
            BatchThreads = Math.Max(1, Environment.ProcessorCount - 1)
        };

        _modelWeights = LLamaWeights.LoadFromFile(modelParams);
        _context = _modelWeights.CreateContext(modelParams);
        _executor = new InteractiveExecutor(_context);

        // 1. Inietta il System Prompt una sola volta all'avvio
        var systemPrompt = "<|system|>\n"
            + "Sei un estrattore di intent CRUD. Rispondi solo con JSON valido.\n"
            + "Formato richiesto: {\"action\":\"Create|Read|Update|Delete\",\"subject\":\"...\",\"humanPhrase\":\"...\"}\n";

        var dummyParams = new InferenceParams { MaxTokens = 0 };
        _executor.InferAsync(systemPrompt, dummyParams).ToBlockingEnumerable().ToList();

        // 2. Salva lo stato in un file di cache temporaneo
        _stateFilePath = Path.Combine(Path.GetTempPath(), $"underdog_state_{Guid.NewGuid():N}.bin");
        _executor.SaveState(_stateFilePath);
    }

    public async Task<string> InferRawJsonAsync(string userSentence, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Ripristina lo stato del System Prompt originale instantaneamente
            await _executor.LoadState(_stateFilePath);

            var prompt = $"<|user|>\n{userSentence}\n<|assistant|>\n";

            var inferenceParams = new InferenceParams
            {
                MaxTokens = _options.MaxTokens,
                AntiPrompts = ["<|user|>", "<|eot_id|>", "<|end_of_text|>"],
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = 0.0f,
                    TopP = 0.1f
                }
            };

            var result = string.Empty;
            await foreach (var token in _executor.InferAsync(prompt, inferenceParams, cancellationToken))
            {
                result += token;
            }

            return result.Trim();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    public void Dispose()
    {
        if (File.Exists(_stateFilePath))
        {
            try { File.Delete(_stateFilePath); } catch { }
        }
        _context.Dispose();
        _modelWeights.Dispose();
        _semaphore.Dispose();
    }
}