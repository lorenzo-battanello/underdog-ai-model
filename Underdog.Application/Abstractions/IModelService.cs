namespace Underdog.Application.Abstractions;

public interface IModelService
{
    Task<string> InferRawJsonAsync(string userSentence, CancellationToken cancellationToken = default);
}
