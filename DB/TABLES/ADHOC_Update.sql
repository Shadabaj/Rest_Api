/*
    Update an already-created EmployeeMaster table so EmployeeCode is generated as NEO0001, NEO0002, ...
    IMPORTANT:
    - This changes any existing EmployeeCode values to match RowId (e.g. RowId 12 becomes NEO0012).
    - Fixed four-digit numbering supports at most 9,999 employee identity values.
    - Back up the database before running this script.
*/
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.EmployeeMaster', N'U') IS NULL
    THROW 50001, 'dbo.EmployeeMaster does not exist. Run the complete database creation script instead.', 1;
GO

IF EXISTS (SELECT 1 FROM dbo.EmployeeMaster WHERE RowId < 1 OR RowId > 9999)
    THROW 50002, 'Existing RowId values are outside 1-9999. The four-digit EmployeeCode format cannot be applied safely.', 1;
GO

BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1 FROM sys.key_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.EmployeeMaster')
      AND name = N'UQ_EmployeeMaster_EmployeeCode'
)
    ALTER TABLE dbo.EmployeeMaster DROP CONSTRAINT UQ_EmployeeMaster_EmployeeCode;

IF COL_LENGTH(N'dbo.EmployeeMaster', N'EmployeeCode') IS NOT NULL
    ALTER TABLE dbo.EmployeeMaster DROP COLUMN EmployeeCode;

ALTER TABLE dbo.EmployeeMaster
ADD EmployeeCode AS ('NEO' + RIGHT('0000' + CONVERT(VARCHAR(10), RowId), 4)) PERSISTED;

ALTER TABLE dbo.EmployeeMaster
ADD CONSTRAINT UQ_EmployeeMaster_EmployeeCode UNIQUE (EmployeeCode);

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.EmployeeMaster')
      AND name = N'CK_EmployeeMaster_RowId_EmployeeCodeRange'
)
    ALTER TABLE dbo.EmployeeMaster
    ADD CONSTRAINT CK_EmployeeMaster_RowId_EmployeeCodeRange CHECK (RowId BETWEEN 1 AND 9999);

COMMIT TRANSACTION;
GO

-- Verify the generated employee IDs.
SELECT RowId, EmployeeCode, FirstName, LastName
FROM dbo.EmployeeMaster
ORDER BY RowId;
GO
