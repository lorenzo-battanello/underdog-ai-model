# Caching Engine

## Implementazione attuale

La cache e su SQL Server, non in memoria.

Componenti:
- `IntentProcessingService` (orchestrazione)
- `IntentCacheService` (facade applicativa)
- `IntentCacheRepository` (query Dapper su `dbo.AiIntentCache`)

Flow cache:
1. normalizzazione prompt + hash SHA-256
2. lookup `NormalizedPromptHash`
3. hit: ritorno immediato + incremento `HitCount`
4. miss: inferenza modello + salvataggio via `dbo.UpsertAiIntentCache`

## Query principali

- `GetByHashAsync`: `SELECT TOP (1) ... FROM dbo.AiIntentCache WHERE NormalizedPromptHash = @Hash`
- `IncrementHitAsync`: update contatore e `LastAccessedAt`
- `SaveAsync`: `EXEC dbo.UpsertAiIntentCache ...`

## Osservabilita integrata

Per ogni richiesta vengono salvati:
- audit (`dbo.IntentAuditLog`)
- metriche (`dbo.IntentPerformanceMetric`)
- storico versione modello (`dbo.ModelVersionHistory`)