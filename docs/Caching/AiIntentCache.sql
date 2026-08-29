/*
  SQL Server script - AI Intent Cache
  Obiettivo: creare la tabella AiIntentCache con lookup veloce su NormalizedPromptHash.
*/

IF OBJECT_ID('dbo.AiIntentCache', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AiIntentCache
    (
        Id BIGINT IDENTITY(1,1) NOT NULL,
        NormalizedPromptHash VARCHAR(64) NOT NULL,
        OriginalPrompt VARCHAR(500) NOT NULL,
        NormalizedPrompt VARCHAR(500) NOT NULL,
        ParsedAction VARCHAR(20) NOT NULL,
        ParsedSubject VARCHAR(50) NOT NULL,
        RawJsonResponse NVARCHAR(MAX) NOT NULL,
        HumanPhrase VARCHAR(255) NOT NULL,
        HitCount INT NOT NULL CONSTRAINT DF_AiIntentCache_HitCount DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_AiIntentCache_CreatedAt DEFAULT (SYSUTCDATETIME()),
        LastAccessedAt DATETIME2(0) NOT NULL CONSTRAINT DF_AiIntentCache_LastAccessedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_AiIntentCache PRIMARY KEY CLUSTERED (Id)
    );
END;
GO

IF OBJECT_ID('dbo.IntentAuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.IntentAuditLog
    (
        Id BIGINT IDENTITY(1,1) NOT NULL,
        CorrelationId VARCHAR(64) NOT NULL,
        OriginalPrompt VARCHAR(500) NOT NULL,
        NormalizedPromptHash VARCHAR(64) NOT NULL,
        RawJsonResponse NVARCHAR(MAX) NOT NULL,
        HumanPhrase VARCHAR(255) NOT NULL,
        FromCache BIT NOT NULL,
        Status VARCHAR(20) NOT NULL,
        ErrorMessage NVARCHAR(1000) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_IntentAuditLog_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_IntentAuditLog PRIMARY KEY CLUSTERED (Id)
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_IntentAuditLog_CorrelationId'
      AND object_id = OBJECT_ID('dbo.IntentAuditLog')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_IntentAuditLog_CorrelationId
        ON dbo.IntentAuditLog (CorrelationId);
END;
GO

IF OBJECT_ID('dbo.IntentPerformanceMetric', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.IntentPerformanceMetric
    (
        Id BIGINT IDENTITY(1,1) NOT NULL,
        CorrelationId VARCHAR(64) NOT NULL,
        DurationMs INT NOT NULL,
        FromCache BIT NOT NULL,
        IsSuccess BIT NOT NULL,
        ErrorCode VARCHAR(100) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_IntentPerformanceMetric_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_IntentPerformanceMetric PRIMARY KEY CLUSTERED (Id)
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_IntentPerformanceMetric_CreatedAt'
      AND object_id = OBJECT_ID('dbo.IntentPerformanceMetric')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_IntentPerformanceMetric_CreatedAt
        ON dbo.IntentPerformanceMetric (CreatedAt);
END;
GO

IF OBJECT_ID('dbo.ModelVersionHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ModelVersionHistory
    (
        Id BIGINT IDENTITY(1,1) NOT NULL,
        ModelVersion VARCHAR(50) NOT NULL,
        ModelPath VARCHAR(500) NOT NULL,
        ModelFileHash VARCHAR(64) NOT NULL,
        FirstSeenAt DATETIME2(0) NOT NULL CONSTRAINT DF_ModelVersionHistory_FirstSeenAt DEFAULT (SYSUTCDATETIME()),
        LastSeenAt DATETIME2(0) NOT NULL CONSTRAINT DF_ModelVersionHistory_LastSeenAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ModelVersionHistory PRIMARY KEY CLUSTERED (Id)
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_ModelVersionHistory_Version_Hash'
      AND object_id = OBJECT_ID('dbo.ModelVersionHistory')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_ModelVersionHistory_Version_Hash
        ON dbo.ModelVersionHistory (ModelVersion, ModelFileHash);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_AiIntentCache_NormalizedPromptHash'
      AND object_id = OBJECT_ID('dbo.AiIntentCache')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_AiIntentCache_NormalizedPromptHash
        ON dbo.AiIntentCache (NormalizedPromptHash);
END;
GO

/*
  Upsert cache entry by hash.
  Se l'hash esiste: incrementa HitCount e aggiorna LastAccessedAt.
  Se non esiste: inserisce un nuovo record.
*/
CREATE OR ALTER PROCEDURE dbo.UpsertAiIntentCache
    @NormalizedPromptHash VARCHAR(64),
    @OriginalPrompt VARCHAR(500),
    @NormalizedPrompt VARCHAR(500),
    @ParsedAction VARCHAR(20),
    @ParsedSubject VARCHAR(50),
    @RawJsonResponse NVARCHAR(MAX),
    @HumanPhrase VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AiIntentCache
    SET
        HitCount = HitCount + 1,
        LastAccessedAt = SYSUTCDATETIME()
    WHERE NormalizedPromptHash = @NormalizedPromptHash;

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.AiIntentCache
        (
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
        )
        VALUES
        (
            @NormalizedPromptHash,
            @OriginalPrompt,
            @NormalizedPrompt,
            @ParsedAction,
            @ParsedSubject,
            @RawJsonResponse,
            @HumanPhrase,
            1,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
    END;
END;
GO
