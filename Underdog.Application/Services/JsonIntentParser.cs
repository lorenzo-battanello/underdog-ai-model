using System.Text.Json;
using Underdog.Application.Models;

namespace Underdog.Application.Services;

internal static class JsonIntentParser
{
    public static ParsedIntent Parse(string rawJson)
    {
        using var document = JsonSerializer.Deserialize<JsonDocument>(rawJson);
        var root = document.RootElement;

        var action = TryRead(root, "action") ?? TryRead(root, "azione") ?? "Unknown";
        var subject = TryRead(root, "subject") ?? TryRead(root, "entity") ?? TryRead(root, "risorsa") ?? "Unknown";
        var id = TryRead(root, "id") ?? TryRead(root, "oggetto") ?? "Unknown";
        var payload = TryRead(root, "payload") ?? TryRead(root, "dati") ?? string.Empty;
        var humanPhrase = TryRead(root, "humanPhrase") ?? TryRead(root, "human_phrase") ?? BuildHumanPhrase(action, subject);

        return new ParsedIntent
        {
            Action = action,
            Subject = subject,
            Id = id,
            Payload = payload,
            HumanPhrase = humanPhrase
        };
    }

    private static string? TryRead(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string BuildHumanPhrase(string action, string subject)
    {
        return $"Vuoi che esegua l'azione {action} su {subject}?";
    }
}
