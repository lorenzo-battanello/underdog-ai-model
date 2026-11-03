# UNDERDOG AI Model

Underdog AI Model trasforma testo naturale in intent strutturati CRUD usando un modello locale GGUF.

## Input / Output

- Input: frase utente (es. `"Crea un ticket"`).
- Output API:

```json
{
	"parsedObject": {
		"action": "Create",
		"entity": "Ticket"
	},
	"humanPhrase": "Vuoi che esegua l'azione Create su Ticket?",
	"fromCache": false
}
```

## Struttura

### Underdog.API
- Espone endpoint HTTP.
- Valida il payload.
- Invia la richiesta a MediatR (`ProcessIntentQuery`).

### Underdog.Application
- Core applicativo CQRS + Mediator + DI + Repository Pattern.
- Orchestrazione intent (`IntentProcessingService`).
- Inferenza modello GGUF singleton (`GgufModelService`).
- Cache intent su SQL Server (`IntentCacheRepository`).
- Observability su SQL Server (`IntentObservabilityRepository`):
	- Audit
	- Metriche performance
	- Storico versioni modello

## Workflow

1. `POST /api/intent` con `userPrompt`.
2. `IntentController` invia query a MediatR.
3. `IntentProcessingService` normalizza testo e calcola hash SHA-256.
4. Ricerca hash in `AiIntentCache`.
5. Cache hit:
	 - incrementa `HitCount`
	 - restituisce risultato da cache
6. Cache miss:
	 - esegue inferenza GGUF
	 - salva risultato in cache tramite `dbo.UpsertAiIntentCache`
7. Per ogni richiesta salva:
	 - Audit in `IntentAuditLog`
	 - Metriche in `IntentPerformanceMetric`
	 - Versione modello in `ModelVersionHistory`

## Database

Script SQL Server: `docs/Caching/AiIntentCache.sql`

Tabelle create:
- `dbo.AiIntentCache`
- `dbo.IntentAuditLog`
- `dbo.IntentPerformanceMetric`
- `dbo.ModelVersionHistory`

## Configurazione

Sezioni `appsettings` usate:
- `AiModel`
	- `ModelPath`
	- `ModelVersion`
	- `ContextSize`
	- `GpuLayerCount`
	- `MaxTokens`
- `IntentCache`
	- `ConnectionString` (SQL Server)
- `GeminiModel`
	- `ApiKey`

### File `.env` (segreti)

Le credenziali (API key e connection string) non sono in `appsettings.json`: vanno in un file `.env`, ignorato da git.

1. Copia `.env.example` in `Underdog.API/.env`.
2. Valorizza `GeminiModel__ApiKey` e `IntentCache__ConnectionString`.

Il file viene caricato all'avvio (`DotNetEnv`) e sovrascrive `appsettings.json`; `__` separa sezione e chiave. In produzione si possono usare direttamente variabili d'ambiente con gli stessi nomi.
