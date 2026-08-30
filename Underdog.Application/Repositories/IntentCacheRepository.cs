using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Underdog.Application.Abstractions;
using Underdog.Application.Configuration;
using Underdog.Application.Data;
using Underdog.Application.Models;

namespace Underdog.Application.Repositories;

public sealed class IntentCacheRepository(IDBMConnection connection) : IIntentCacheRepository
{

    private readonly IDBMConnection _connection = connection;
    public async Task<AiIntentCacheRow?> GetByHashAsync(string normalizedPromptHash, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (1)
                CAST(Id AS BIGINT) AS Id,
                NormalizedPromptHash,
                OriginalPrompt,
                NormalizedPrompt,
                ParsedAction,
                ParsedSubject,
                RawJsonResponse,
                HumanPhrase,
                HitCount,
                CreatedAt,
                LastAccessedAt
            FROM dbo.AiIntentCache
            WHERE NormalizedPromptHash = @Hash
            ORDER BY Id DESC;
            """;

        using var connection = _connection.OpenConnection();
        return await connection.QueryFirstOrDefaultAsync<AiIntentCacheRow>(
            new CommandDefinition(sql, new { Hash = normalizedPromptHash }, cancellationToken: cancellationToken));
    }

    public async Task IncrementHitAsync(ulong id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.AiIntentCache
            SET HitCount = HitCount + 1,
                LastAccessedAt = SYSUTCDATETIME()
            WHERE Id = @Id;
            """;

        using var connection = _connection.OpenConnection();
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = (long)id },
            cancellationToken: cancellationToken));
    }

    public async Task SaveAsync(string originalPrompt, string normalizedPrompt, string normalizedPromptHash, ParsedIntent parsedIntent, string rawJsonResponse, CancellationToken cancellationToken = default)
    {
        const string sql = "EXEC dbo.UpsertAiIntentCache @NormalizedPromptHash, @OriginalPrompt, @NormalizedPrompt, @ParsedAction, @ParsedSubject, @RawJsonResponse, @HumanPhrase;";

        using var connection = _connection.OpenConnection();
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                NormalizedPromptHash = normalizedPromptHash,
                OriginalPrompt = originalPrompt,
                NormalizedPrompt = normalizedPrompt,
                ParsedAction = parsedIntent.Action,
                ParsedSubject = parsedIntent.Subject,
                RawJsonResponse = rawJsonResponse,
                HumanPhrase = parsedIntent.HumanPhrase
            },
            cancellationToken: cancellationToken));
    }    
}
