using LLama;
using LLama.Common;
using Microsoft.Extensions.Options;
using Underdog.Application.Abstractions;
using Underdog.Application.Configuration;

namespace Underdog.Application.Services;

public sealed class GgufModelService : IGgufModelService, IDisposable
{
    private readonly AiModelOptions _options;
    private readonly LLamaWeights _modelWeights;
    private readonly StatelessExecutor _executor;

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
            ContextSize = _options.ContextSize,
            GpuLayerCount = _options.GpuLayerCount
        };

        _modelWeights = LLamaWeights.LoadFromFile(modelParams);
        _executor = new StatelessExecutor(_modelWeights, modelParams);
    }

    public async Task<string> InferRawJsonAsync(string userSentence, CancellationToken cancellationToken = default)
    {
        var prompt = "<|system|>\n"
            + "Sei un estrattore di intent CRUD. Rispondi solo con JSON valido.\n"
            + "Formato richiesto: {\"action\":\"Create|Read|Update|Delete\",\"subject\":\"...\",\"humanPhrase\":\"...\"}\n"
            + "<|user|>\n"
            + userSentence + "\n"
            + "<|assistant|>\n";

        var inferenceParams = new InferenceParams
        {
            MaxTokens = _options.MaxTokens,
            AntiPrompts = ["<|user|>"]
        };

        var result = string.Empty;
        await foreach (var token in _executor.InferAsync(prompt, inferenceParams, cancellationToken))
        {
            result += token;
        }

        return result.Trim();
    }

    public void Dispose()
    {
        _modelWeights.Dispose();
    }
}
