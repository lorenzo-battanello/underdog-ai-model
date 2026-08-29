namespace Underdog.Application.Configuration;

public sealed class AiModelOptions
{
    public const string SectionName = "AiModel";

    public string ModelPath { get; set; } = string.Empty;

    public string ModelVersion { get; set; } = "v1";

    public uint ContextSize { get; set; } = 2048;

    public int GpuLayerCount { get; set; } = 0;

    public int MaxTokens { get; set; } = 128;
}
