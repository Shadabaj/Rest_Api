IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LoginOtps')
BEGIN
    CREATE TABLE dbo.LoginOtps
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoginOtps PRIMARY KEY,
        UserId      INT NOT NULL,
        UserName    NVARCHAR(100) NOT NULL,
        Email       NVARCHAR(256) NOT NULL,
        CodeHash    NVARCHAR(128) NOT NULL,
        ExpiresAt   DATETIME2(0) NOT NULL,
        CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_LoginOtps_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ConsumedAt  DATETIME2(0) NULL,
        CreatedByIp VARCHAR(45) NULL,
        CONSTRAINT FK_LoginOtps_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_LoginOtps_UserName_ExpiresAt ON dbo.LoginOtps (UserName, ExpiresAt);
END
GO
