Questo documento descrive lo schema SQL Server usato dal progetto per cache intent, audit, metriche e storico versioni modello.

---

### Tabella `dbo.AiIntentCache`

| Colonna | Tipo Dati | Descrizione / Regola |
| --- | --- | --- |
| **Id** | `BIGINT` | Chiave primaria autoincrementante. |
| **NormalizedPromptHash** | `VARCHAR(64)` | *(Fondamentale)* Hash **SHA-256** della frase pulita. Usato per la ricerca rapida tramite indice. |
| **OriginalPrompt** | `VARCHAR(500)` | La frase esatta inviata dall'utente (utile per audit e debug). |
| **NormalizedPrompt** | `VARCHAR(500)` | Frase pulita (minuscola, senza punteggiatura/spazi doppi). |
| **ParsedAction** | `VARCHAR(20)` | L'azione CRUD estratta (es. `Create`, `Delete`). Permette statistiche rapide. |
| **ParsedSubject** | `VARCHAR(50)` | L'entità estratta (es. `Ticket`, `Database`). |
| **RawJsonResponse** | `NVARCHAR(MAX)` | Il JSON completo estratto dall'AI. |
| **HumanPhrase** | `VARCHAR(255)` | La frase amichevole generata dall'AI da mostrare all'utente. |
| **HitCount** | `INT` | Counter di quante volte questa cache è stata usata (default: `1`). |
| **CreatedAt** | `DATETIME2(0)` | Data e ora della prima generazione via AI. |
| **LastAccessedAt** | `DATETIME2(0)` | Data e ora dell'ultimo utilizzo. |

---

### Indici principali

- `UX_AiIntentCache_NormalizedPromptHash` (unico)
- lookup veloce su hash SHA-256 normalizzato

### Altre tabelle create

#### `dbo.IntentAuditLog`
- traccia esito richiesta (`Success`/`Error`)
- contiene `CorrelationId`, prompt, hash, risposta raw, eventuale errore
- indice: `IX_IntentAuditLog_CorrelationId`

#### `dbo.IntentPerformanceMetric`
- traccia tempi in millisecondi, hit/miss cache, successo/errore
- indice: `IX_IntentPerformanceMetric_CreatedAt`

#### `dbo.ModelVersionHistory`
- storico versioni modello osservate in esecuzione
- campi: `ModelVersion`, `ModelPath`, `ModelFileHash`, first/last seen
- indice unico: `UX_ModelVersionHistory_Version_Hash`

---

### Stored procedure usata dal codice

Il salvataggio cache usa:
- `dbo.UpsertAiIntentCache`

Comportamento:
1. se hash esiste: incrementa `HitCount` e aggiorna `LastAccessedAt`
2. se hash non esiste: inserisce nuova riga in `dbo.AiIntentCache`

---

### Normalizzazione e hash

Prima del lookup cache:

```csharp
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public static class TextNormalizer
{
    public static (string NormalizedText, string Hash) CleanAndHash(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return (string.Empty, string.Empty);

        // 1. Minuscolo, rimozione punteggiatura e spazi doppi
        string cleaned = text.ToLowerInvariant().Trim();
        cleaned = Regex.Replace(cleaned, @"[^\w\s]", ""); // Rimuove punteggiatura
        cleaned = Regex.Replace(cleaned, @"\s+", " ");    // Spazi multipli -> spazio singolo

        // 2. Calcolo Hash SHA-256
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cleaned));
        string hash = Convert.ToHexString(bytes);

        return (cleaned, hash);
    }
}

```

### Flusso query applicativo

1. Utente invia: *"Crea un nuovo TICKET per l'errore DB!!"*
2. Il servizio calcola `NormalizedText` = `"crea un nuovo ticket per lerrore db"` e genera l'hash.
3. Cerca sul DB: `SELECT TOP (1) ... FROM dbo.AiIntentCache WHERE NormalizedPromptHash = @Hash`.
4. **Se esiste:** incrementa `HitCount`, aggiorna `LastAccessedAt`, restituisce il risultato.
5. **Se non esiste:** chiama modello GGUF, salva via `EXEC dbo.UpsertAiIntentCache`, restituisce il risultato.

Riferimento script completo: `docs/Caching/AiIntentCache.sql`.