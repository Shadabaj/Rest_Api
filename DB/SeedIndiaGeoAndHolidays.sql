SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

-- India + sample states/cities for dropdowns
IF NOT EXISTS (SELECT 1 FROM dbo.Country WHERE CountryName = N'India')
    INSERT INTO dbo.Country (CountryName) VALUES (N'India');

DECLARE @IndiaId INT = (SELECT TOP 1 RowId FROM dbo.Country WHERE CountryName = N'India');

IF NOT EXISTS (SELECT 1 FROM dbo.State WHERE CountryId = @IndiaId AND StateName = N'Maharashtra')
    INSERT INTO dbo.State (CountryId, StateName) VALUES (@IndiaId, N'Maharashtra');
IF NOT EXISTS (SELECT 1 FROM dbo.State WHERE CountryId = @IndiaId AND StateName = N'Karnataka')
    INSERT INTO dbo.State (CountryId, StateName) VALUES (@IndiaId, N'Karnataka');
IF NOT EXISTS (SELECT 1 FROM dbo.State WHERE CountryId = @IndiaId AND StateName = N'Delhi')
    INSERT INTO dbo.State (CountryId, StateName) VALUES (@IndiaId, N'Delhi');
IF NOT EXISTS (SELECT 1 FROM dbo.State WHERE CountryId = @IndiaId AND StateName = N'Tamil Nadu')
    INSERT INTO dbo.State (CountryId, StateName) VALUES (@IndiaId, N'Tamil Nadu');
IF NOT EXISTS (SELECT 1 FROM dbo.State WHERE CountryId = @IndiaId AND StateName = N'Gujarat')
    INSERT INTO dbo.State (CountryId, StateName) VALUES (@IndiaId, N'Gujarat');

DECLARE @MH INT = (SELECT RowId FROM dbo.State WHERE CountryId = @IndiaId AND StateName = N'Maharashtra');
DECLARE @KA INT = (SELECT RowId FROM dbo.State WHERE CountryId = @IndiaId AND StateName = N'Karnataka');
DECLARE @DL INT = (SELECT RowId FROM dbo.State WHERE CountryId = @IndiaId AND StateName = N'Delhi');

IF NOT EXISTS (SELECT 1 FROM dbo.City WHERE StateId = @MH AND CityName = N'Mumbai')
    INSERT INTO dbo.City (StateId, CityName) VALUES (@MH, N'Mumbai');
IF NOT EXISTS (SELECT 1 FROM dbo.City WHERE StateId = @MH AND CityName = N'Pune')
    INSERT INTO dbo.City (StateId, CityName) VALUES (@MH, N'Pune');
IF NOT EXISTS (SELECT 1 FROM dbo.City WHERE StateId = @KA AND CityName = N'Bengaluru')
    INSERT INTO dbo.City (StateId, CityName) VALUES (@KA, N'Bengaluru');
IF NOT EXISTS (SELECT 1 FROM dbo.City WHERE StateId = @DL AND CityName = N'New Delhi')
    INSERT INTO dbo.City (StateId, CityName) VALUES (@DL, N'New Delhi');

-- National holidays for current year (CountryId = India)
DECLARE @Y INT = YEAR(GETUTCDATE());

;WITH Holidays(Name, Dt, Optional) AS (
    SELECT N'Republic Day', DATEFROMPARTS(@Y,1,26), 0 UNION ALL
    SELECT N'Holi', DATEFROMPARTS(@Y,3,14), 0 UNION ALL
    SELECT N'Good Friday', DATEFROMPARTS(@Y,4,3), 1 UNION ALL
    SELECT N'Eid ul-Fitr', DATEFROMPARTS(@Y,3,31), 0 UNION ALL
    SELECT N'Independence Day', DATEFROMPARTS(@Y,8,15), 0 UNION ALL
    SELECT N'Raksha Bandhan', DATEFROMPARTS(@Y,8,9), 1 UNION ALL
    SELECT N'Janmashtami', DATEFROMPARTS(@Y,8,15), 1 UNION ALL
    SELECT N'Ganesh Chaturthi', DATEFROMPARTS(@Y,8,27), 1 UNION ALL
    SELECT N'Gandhi Jayanti', DATEFROMPARTS(@Y,10,2), 0 UNION ALL
    SELECT N'Dussehra', DATEFROMPARTS(@Y,10,20), 0 UNION ALL
    SELECT N'Diwali', DATEFROMPARTS(@Y,11,8), 0 UNION ALL
    SELECT N'Guru Nanak Jayanti', DATEFROMPARTS(@Y,11,15), 1 UNION ALL
    SELECT N'Christmas', DATEFROMPARTS(@Y,12,25), 0
)
INSERT INTO dbo.CompanyHolidays (HolidayName, HolidayDate, CountryId, IsOptional, IsActive)
SELECT h.Name, h.Dt, @IndiaId, h.Optional, 1
FROM Holidays h
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CompanyHolidays c
    WHERE c.HolidayDate = h.Dt AND c.HolidayName = h.Name AND ISNULL(c.CountryId,0) = @IndiaId
);

-- Trip plans
IF OBJECT_ID('dbo.TripPlans') IS NULL
BEGIN
    CREATE TABLE dbo.TripPlans
    (
        TripPlanId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TripPlans PRIMARY KEY,
        Title          NVARCHAR(200) NOT NULL,
        Description    NVARCHAR(1000) NULL,
        StartDate      DATE NOT NULL,
        EndDate        DATE NOT NULL,
        TargetAmount   DECIMAL(18,2) NOT NULL,
        CollectedAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_TripPlans_Collected DEFAULT (0),
        CreatedByUserId INT NULL,
        IsActive       BIT NOT NULL CONSTRAINT DF_TripPlans_IsActive DEFAULT (1),
        CreatedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_TripPlans_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_TripPlans_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(Id)
    );
END
GO

IF OBJECT_ID('dbo.TripPayments') IS NULL
BEGIN
    CREATE TABLE dbo.TripPayments
    (
        TripPaymentId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TripPayments PRIMARY KEY,
        TripPlanId      INT NOT NULL,
        UserId          INT NOT NULL,
        Amount          DECIMAL(18,2) NOT NULL,
        PaymentMethod   NVARCHAR(50) NOT NULL,
        PaymentStatus   NVARCHAR(20) NOT NULL CONSTRAINT DF_TripPayments_Status DEFAULT ('PAID'),
        PaidAt          DATETIME2(0) NOT NULL CONSTRAINT DF_TripPayments_PaidAt DEFAULT (SYSUTCDATETIME()),
        ReferenceCode   NVARCHAR(64) NULL,
        CONSTRAINT FK_TripPayments_Trip FOREIGN KEY (TripPlanId) REFERENCES dbo.TripPlans(TripPlanId),
        CONSTRAINT FK_TripPayments_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END
GO
