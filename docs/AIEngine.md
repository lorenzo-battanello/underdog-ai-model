# AI Engine

## Pacchetti

- `LLamaSharp`
- `LLamaSharp.Backend.Cpu`

## Implementazione attuale

Il motore GGUF e implementato in `GgufModelService` come servizio singleton:

- carica il modello una sola volta in RAM (`LLamaWeights`)
- usa `StatelessExecutor`
- riceve parametri da configurazione `AiModel`

Parametri configurabili:
- `ModelPath`
- `ModelVersion`
- `ContextSize`
- `GpuLayerCount`
- `MaxTokens`

Prompt behavior:
- system prompt vincola output JSON
- anti-prompt `"<|user|>"`

## Dependency Injection

Registrazione corrente in Application DI:

```csharp
services.AddSingleton<IGgufModelService, GgufModelService>();
```

L'API consuma il servizio tramite pipeline:
`IntentController -> MediatR -> IntentProcessingService -> IGgufModelService`.