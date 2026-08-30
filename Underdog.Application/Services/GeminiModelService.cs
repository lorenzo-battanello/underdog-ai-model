using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using Underdog.Application.Abstractions;

namespace Underdog.Application.Services;

public sealed class GeminiModelService : IModelService
{
    private readonly GenerativeModel _model;

    public GeminiModelService(IConfiguration configuration)
    {
        var apiKey = configuration["GeminiModel:ApiKey"] 
                     ?? throw new InvalidOperationException("API Key di Gemini non configurata.");

        var googleAI = new GoogleAI(apiKey);
        
        // Utilizza il modello Flash aggiornato e stabile
        _model = googleAI.GenerativeModel(Model.GeminiFlashLiteLatest);
    }

    public async Task<string> InferRawJsonAsync(string userSentence, CancellationToken cancellationToken = default)
    {
        var prompt = "Sei un estrattore di intent CRUD. Restituisci ESCLUSIVAMENTE un JSON valido con questo formato: {\"action\":\"Create|Read|Update|Delete\",\"subject\":\"...\",\"payload\":\"...\",\"id\":\"...\",\"humanPhrase\":\"...\"}\nIn humanPhrase metti una frase che rappresenti in modo naturale la risposta all'intento dell'utente.\nFrase utente: " + userSentence;

        // Configurazione per forzare un output pulito
        var request = new GenerateContentRequest
        {
            Contents = [new Content { Parts = [new Part { Text = prompt }] }],
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.0f,
                ResponseMimeType = "application/json"
            }
        };

        var response = await _model.GenerateContent(request);
        return response.Text?.Trim() ?? "{}";
    }
}