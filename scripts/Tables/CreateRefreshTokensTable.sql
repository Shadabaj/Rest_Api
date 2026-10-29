IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
        UserId               INT NOT NULL,
        TokenHash            NVARCHAR(128) NOT NULL,
        ExpiresAt            DATETIME2(0) NOT NULL,
        CreatedAt            DATETIME2(0) NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT (SYSUTCDATETIME()),
        RevokedAt            DATETIME2(0) NULL,
        ReplacedByTokenHash  NVARCHAR(128) NULL,
        CreatedByIp          VARCHAR(45) NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_RefreshTokens_TokenHash UNIQUE (TokenHash)
    );

    CREATE INDEX IX_RefreshTokens_User_ExpiresAt ON dbo.RefreshTokens (UserId, ExpiresAt);

END
GO
