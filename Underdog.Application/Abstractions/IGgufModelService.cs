namespace Underdog.Application.Abstractions;

public interface IGgufModelService
{
    Task<string> InferRawJsonAsync(string userSentence, CancellationToken cancellationToken = default);
}
