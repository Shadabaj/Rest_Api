CREATE OR ALTER FUNCTION dbo.fn_GetNextEmployeeCode()
RETURNS varchar(50)
AS
BEGIN
    DECLARE @Next int;

    SELECT @Next = ISNULL(MAX(CAST(SUBSTRING(EmployeeCode, 4, 4) AS int)), 0) + 1
    FROM dbo.EmployeeMaster
    WHERE EmployeeCode LIKE 'NEO[0-9][0-9][0-9][0-9]';

    IF @Next > 9999
        RETURN NULL;   -- limit reached, insert fails on NOT NULL

    RETURN 'NEO' + RIGHT('0000' + CAST(@Next AS varchar(10)), 4);
END;

