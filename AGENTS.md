## Technical structure

### Architettura 
- CQRS (Command Query Responsibility Segregation)
- Dependency Injection (DI)
- Mediator
- Repository pattern for data access


### Underdog.API
Il progetto API controller che serve unicamente da interfaccia tra utente e servizio AI Model.

- API Controller (IntentController):
  - Riceve la frase dell'utente tramite POST HTTP.
  - Valida il payload (`UserPrompt`).
  - Inoltra la richiesta a MediatR (`ProcessIntentQuery`).

### Underdog.Application
E' il core del sistema, in base al prompt passato dall'utente, contatta l'AI Model e restituisce output elaborato e human phrase.

- Servizio Scoped (IntentCacheService):
  - Facade applicativa della cache.
  - Usa `IIntentCacheRepository` per leggere/scrivere dati su SQL Server.

- Servizio Singleton (GgufModelService):
  - È obbligatoriamente Singleton per caricare i file pesanti .gguf in RAM (usando LLamaSharp) una sola volta all'avvio (warm-up).
  - Esegue l'inferenza (se il CacheService non trova corrispondenze).

- Servizio Scoped (IntentProcessingService):
  - Orchestrazione end-to-end della richiesta.
  - Normalizza il prompt e calcola hash SHA-256.
  - Controlla cache, chiama modello in caso di miss, salva output cache.
  - Registra audit/metriche/storico versione modello.

- Repository Scoped (IntentCacheRepository):
  - Accesso tabella `dbo.AiIntentCache`.
  - Lettura hash, incremento hit, salvataggio via `dbo.UpsertAiIntentCache`.

- Repository Scoped (IntentObservabilityRepository):
  - Audit su `dbo.IntentAuditLog`.
  - Metriche su `dbo.IntentPerformanceMetric`.
  - Storico versioni su `dbo.ModelVersionHistory`.

- Flusso di ritorno: Il risultato del modello viene passato al IntentCacheService per essere salvato nel DB e, infine, il controller restituisce l'oggetto finale:
    
    ```json
    {
        "parsedObject": { "action": "Update", "entity": "Ticket" },
      "humanPhrase": "Vuoi che esegua l'azione Update su Ticket?",
      "fromCache": false
    }
    ```

