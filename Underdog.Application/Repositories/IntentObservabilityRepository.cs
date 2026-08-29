using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Underdog.Application.Abstractions;
using Underdog.Application.Configuration;

namespace Underdog.Application.Repositories;

public sealed class IntentObservabilityRepository : IIntentObservabilityRepository
{
    private readonly CacheOptions _options;

    public IntentObservabilityRepository(IOptions<CacheOptions> options)
    {
        _options = options.Value;
    }

    public async Task SaveAuditAsync(string correlationId, string originalPrompt, string normalizedPromptHash, string rawJsonResponse, string humanPhrase, bool fromCache, string status, string? errorMessage, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.IntentAuditLog
            (
                CorrelationId,
                OriginalPrompt,
                NormalizedPromptHash,
                RawJsonResponse,
                HumanPhrase,
                FromCache,
                Status,
                ErrorMessage,
                CreatedAt
            )
            VALUES
            (
                @CorrelationId,
                @OriginalPrompt,
                @NormalizedPromptHash,
                @RawJsonResponse,
                @HumanPhrase,
                @FromCache,
                @Status,
                @ErrorMessage,
                SYSUTCDATETIME()
            );
            """;

        using var connection = OpenConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            CorrelationId = correlationId,
            OriginalPrompt = originalPrompt,
            NormalizedPromptHash = normalizedPromptHash,
            RawJsonResponse = rawJsonResponse,
            HumanPhrase = humanPhrase,
            FromCache = fromCache,
            Status = status,
            ErrorMessage = errorMessage
        }, cancellationToken: cancellationToken));
    }

    public async Task SaveMetricAsync(string correlationId, int durationMs, bool fromCache, bool isSuccess, string? errorCode, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.IntentPerformanceMetric
            (
                CorrelationId,
                DurationMs,
                FromCache,
                IsSuccess,
                ErrorCode,
                CreatedAt
            )
            VALUES
            (
                @CorrelationId,
                @DurationMs,
                @FromCache,
                @IsSuccess,
                @ErrorCode,
                SYSUTCDATETIME()
            );
            """;

        using var connection = OpenConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            CorrelationId = correlationId,
            DurationMs = durationMs,
            FromCache = fromCache,
            IsSuccess = isSuccess,
            ErrorCode = errorCode
        }, cancellationToken: cancellationToken));
    }

    public async Task UpsertModelVersionAsync(string modelVersion, string modelPath, string modelFileHash, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.ModelVersionHistory
            SET
                LastSeenAt = SYSUTCDATETIME(),
                ModelPath = @ModelPath
            WHERE ModelVersion = @ModelVersion
              AND ModelFileHash = @ModelFileHash;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.ModelVersionHistory
                (
                    ModelVersion,
                    ModelPath,
                    ModelFileHash,
                    FirstSeenAt,
                    LastSeenAt
                )
                VALUES
                (
                    @ModelVersion,
                    @ModelPath,
                    @ModelFileHash,
                    SYSUTCDATETIME(),
                    SYSUTCDATETIME()
                );
            END;
            """;

        using var connection = OpenConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            ModelVersion = modelVersion,
            ModelPath = modelPath,
            ModelFileHash = modelFileHash
        }, cancellationToken: cancellationToken));
    }

    private IDbConnection OpenConnection()
    {
        var connection = new SqlConnection(_options.ConnectionString);
        connection.Open();
        return connection;
    }
}
