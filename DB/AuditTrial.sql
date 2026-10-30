IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditTrial')
BEGIN
    CREATE TABLE dbo.AuditTrial
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditTrial_ID PRIMARY KEY,
        UserId          INT NOT NULL,
        UserName        VARCHAR(128) NOT NULL,
        ControllerName  NVARCHAR(128) NULL,
        ActionName      NVARCHAR(128) NULL,
        CreatedByIp     VARCHAR(45) NULL,
        ExecutionTimeMs INT NULL,
        Timestamp       DATETIME NULL,
        Message         NVARCHAR(400) NULL,
        CONSTRAINT FK_AuditTrial_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AuditTrial_UserName ON dbo.AuditTrial (UserName);
END
GO

-- Activity log needs many rows per user; drop unique if an older script created it.
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UQ_AuditTrial_UserName' AND object_id = OBJECT_ID('dbo.AuditTrial')
)
BEGIN
    ALTER TABLE dbo.AuditTrial DROP CONSTRAINT UQ_AuditTrial_UserName;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_AuditTrial_UserName' AND object_id = OBJECT_ID('dbo.AuditTrial')
)
BEGIN
    CREATE INDEX IX_AuditTrial_UserName ON dbo.AuditTrial (UserName);
END
GO
