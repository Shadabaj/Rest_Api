create database keka
/*
    Employee Management & Attendance Tracking - SQL Server starter schema
    Run this script in a NEW / EMPTY target database.

    Design notes:
    - EmployeeMaster stores the HR profile.
    - Users / Roles / UserRoles handle application authentication and authorization.
    - UserSessionHistory records application login/logout sessions.
    - AttendancePunches stores raw work IN/OUT/break events (the audit source of truth).
    - AttendanceDaily stores the calculated daily attendance summary.
    - DATETIME2 timestamps are intended to be stored in UTC. AttendanceDate is the
      employee's local work date. Shift times are local times interpreted using the
      assigned work location's TimeZoneId.
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* 1. Geographic lookup tables */
CREATE TABLE dbo.Country
(
    RowId       INT IDENTITY(1,1) NOT NULL,
    CountryName NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_Country PRIMARY KEY CLUSTERED (RowId),
    CONSTRAINT UQ_Country_CountryName UNIQUE (CountryName)
);
GO

CREATE TABLE dbo.[State]
(
    RowId     INT IDENTITY(1,1) NOT NULL,
    CountryId INT NOT NULL,
    StateName NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_State PRIMARY KEY CLUSTERED (RowId),
    CONSTRAINT UQ_State_Country_StateName UNIQUE (CountryId, StateName),
    -- Composite key lets child tables verify that the state belongs to the selected country.
    CONSTRAINT UQ_State_RowId_CountryId UNIQUE (RowId, CountryId),
    CONSTRAINT FK_State_Country FOREIGN KEY (CountryId)
        REFERENCES dbo.Country (RowId)
);
GO

CREATE TABLE dbo.City
(
    RowId   INT IDENTITY(1,1) NOT NULL,
    StateId INT NOT NULL,
    CityName NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_City PRIMARY KEY CLUSTERED (RowId),
    CONSTRAINT UQ_City_State_CityName UNIQUE (StateId, CityName),
    -- Composite key lets child tables verify that the city belongs to the selected state.
    CONSTRAINT UQ_City_RowId_StateId UNIQUE (RowId, StateId),
    CONSTRAINT FK_City_State FOREIGN KEY (StateId)
        REFERENCES dbo.[State] (RowId)
);
GO

/* 2. Organization setup */
CREATE TABLE dbo.WorkLocations
(
    WorkLocationId INT IDENTITY(1,1) NOT NULL,
    LocationCode   VARCHAR(20) NOT NULL,
    LocationName   NVARCHAR(150) NOT NULL,
    AddressLine1   NVARCHAR(200) NULL,
    AddressLine2   NVARCHAR(200) NULL,
    CountryId      INT NULL,
    StateId        INT NULL,
    CityId         INT NULL,
    TimeZoneId     NVARCHAR(100) NOT NULL,
    IsActive       BIT NOT NULL CONSTRAINT DF_WorkLocations_IsActive DEFAULT (1),
    CreatedDate    DATETIME2(0) NOT NULL CONSTRAINT DF_WorkLocations_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_WorkLocations PRIMARY KEY CLUSTERED (WorkLocationId),
    CONSTRAINT UQ_WorkLocations_LocationCode UNIQUE (LocationCode),
    CONSTRAINT FK_WorkLocations_Country FOREIGN KEY (CountryId)
        REFERENCES dbo.Country (RowId),
    CONSTRAINT FK_WorkLocations_StateCountry FOREIGN KEY (StateId, CountryId)
        REFERENCES dbo.[State] (RowId, CountryId),
    CONSTRAINT FK_WorkLocations_CityState FOREIGN KEY (CityId, StateId)
        REFERENCES dbo.City (RowId, StateId),
    CONSTRAINT CK_WorkLocations_LocationHierarchy CHECK
        ((StateId IS NULL OR CountryId IS NOT NULL) AND (CityId IS NULL OR StateId IS NOT NULL))
);
GO

CREATE TABLE dbo.Departments
(
    DepartmentId   INT IDENTITY(1,1) NOT NULL,
    DepartmentCode VARCHAR(20) NOT NULL,
    DepartmentName NVARCHAR(100) NOT NULL,
    Description    NVARCHAR(500) NULL,
    IsActive       BIT NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT (1),
    CreatedDate    DATETIME2(0) NOT NULL CONSTRAINT DF_Departments_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Departments PRIMARY KEY CLUSTERED (DepartmentId),
    CONSTRAINT UQ_Departments_Code UNIQUE (DepartmentCode),
    CONSTRAINT UQ_Departments_Name UNIQUE (DepartmentName)
);
GO

CREATE TABLE dbo.Designations
(
    DesignationId   INT IDENTITY(1,1) NOT NULL,
    DesignationCode VARCHAR(20) NOT NULL,
    DesignationName NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Designations_IsActive DEFAULT (1),
    CONSTRAINT PK_Designations PRIMARY KEY CLUSTERED (DesignationId),
    CONSTRAINT UQ_Designations_Code UNIQUE (DesignationCode),
    CONSTRAINT UQ_Designations_Name UNIQUE (DesignationName)
);
GO

/* 3. Employee profile (retains the main columns from the supplied EmployeeMaster entity) */
CREATE TABLE dbo.EmployeeMaster
(
    RowId             INT IDENTITY(1,1) NOT NULL,
    EmployeeCode      VARCHAR(30) unique NOT NULL,
    FirstName         NVARCHAR(100) NOT NULL,
    LastName          NVARCHAR(100) NOT NULL,
    CountryId         INT NULL,
    StateId           INT NULL,
    CityId            INT NULL,
    EmailAddress      NVARCHAR(254) NULL,
    MobileNumber      VARCHAR(25) NULL,
    PanNumber         VARCHAR(20) NULL,
    PassportNumber    VARCHAR(20) NULL,
    ProfileImage      NVARCHAR(500) NULL, -- Store a URL/blob path, not image bytes.
    Gender            TINYINT NULL,       -- Define the numeric mapping in application documentation.
    IsActive          BIT NOT NULL CONSTRAINT DF_EmployeeMaster_IsActive DEFAULT (1),
    DateOfBirth       DATE NOT NULL,
    DateOfJoinee      DATE NOT NULL,
    DepartmentId      INT NULL,
    DesignationId     INT NULL,
    ReportingManagerId INT NULL,
    WorkLocationId    INT NULL,
    DateOfExit        DATE NULL,
    CreatedDate       DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeMaster_CreatedDate DEFAULT (SYSUTCDATETIME()),
    UpdatedDate       DATETIME2(0) NULL,
    IsDeleted         BIT NOT NULL CONSTRAINT DF_EmployeeMaster_IsDeleted DEFAULT (0),
    DeletedDate       DATETIME2(0) NULL,

    CONSTRAINT PK_EmployeeMaster PRIMARY KEY CLUSTERED (RowId),
    CONSTRAINT UQ_EmployeeMaster_EmployeeCode UNIQUE (EmployeeCode),
    CONSTRAINT FK_EmployeeMaster_Country FOREIGN KEY (CountryId)
        REFERENCES dbo.Country (RowId),
    CONSTRAINT FK_EmployeeMaster_StateCountry FOREIGN KEY (StateId, CountryId)
        REFERENCES dbo.[State] (RowId, CountryId),
    CONSTRAINT FK_EmployeeMaster_CityState FOREIGN KEY (CityId, StateId)
        REFERENCES dbo.City (RowId, StateId),
    CONSTRAINT FK_EmployeeMaster_Department FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments (DepartmentId),
    CONSTRAINT FK_EmployeeMaster_Designation FOREIGN KEY (DesignationId)
        REFERENCES dbo.Designations (DesignationId),
    CONSTRAINT FK_EmployeeMaster_ReportingManager FOREIGN KEY (ReportingManagerId)
        REFERENCES dbo.EmployeeMaster (RowId),
    CONSTRAINT FK_EmployeeMaster_WorkLocation FOREIGN KEY (WorkLocationId)
        REFERENCES dbo.WorkLocations (WorkLocationId),
    CONSTRAINT CK_EmployeeMaster_LocationHierarchy CHECK
        ((StateId IS NULL OR CountryId IS NOT NULL) AND (CityId IS NULL OR StateId IS NOT NULL)),
    CONSTRAINT CK_EmployeeMaster_JoinExitDates CHECK
        (DateOfJoinee >= DateOfBirth AND (DateOfExit IS NULL OR DateOfExit >= DateOfJoinee)),
    CONSTRAINT CK_EmployeeMaster_NotOwnManager CHECK
        (ReportingManagerId IS NULL OR ReportingManagerId <> RowId)
);
GO

-- Optional personal fields can be absent, but populated email/PAN/passport values should be unique.
CREATE UNIQUE INDEX UX_EmployeeMaster_EmailAddress
    ON dbo.EmployeeMaster (EmailAddress)
    WHERE EmailAddress IS NOT NULL AND EmailAddress <> N'';
GO
CREATE UNIQUE INDEX UX_EmployeeMaster_PanNumber
    ON dbo.EmployeeMaster (PanNumber)
    WHERE PanNumber IS NOT NULL AND PanNumber <> '';
GO
CREATE UNIQUE INDEX UX_EmployeeMaster_PassportNumber
    ON dbo.EmployeeMaster (PassportNumber)
    WHERE PassportNumber IS NOT NULL AND PassportNumber <> '';
GO

/* 4. Application accounts and authorization */
CREATE TABLE dbo.Users
(
    Id                    INT IDENTITY(1,1) NOT NULL,
    EmployeeId            INT NOT NULL,
    UserName              NVARCHAR(100) NOT NULL,
    PasswordHash          NVARCHAR(500) NOT NULL,
    EmailConfirmed        BIT NOT NULL CONSTRAINT DF_Users_EmailConfirmed DEFAULT (0),
    PhoneNumberConfirmed  BIT NOT NULL CONSTRAINT DF_Users_PhoneNumberConfirmed DEFAULT (0),
    IsActive              BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    IsLockedOut           BIT NOT NULL CONSTRAINT DF_Users_IsLockedOut DEFAULT (0),
    FailedLoginCount      INT NOT NULL CONSTRAINT DF_Users_FailedLoginCount DEFAULT (0),
    LockoutEndAt          DATETIME2(0) NULL,
    LastPasswordChangedAt DATETIME2(0) NULL,
    CreatedDate           DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedDate DEFAULT (SYSUTCDATETIME()),
    UpdatedDate           DATETIME2(0) NULL,
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Users_EmployeeId UNIQUE (EmployeeId),
    CONSTRAINT UQ_Users_UserName UNIQUE (UserName),
    CONSTRAINT FK_Users_EmployeeMaster FOREIGN KEY (EmployeeId)
        REFERENCES dbo.EmployeeMaster (RowId),
    CONSTRAINT CK_Users_FailedLoginCount CHECK (FailedLoginCount >= 0)
);
GO

CREATE TABLE dbo.Roles
(
    Id          INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsSystemRole BIT NOT NULL CONSTRAINT DF_Roles_IsSystemRole DEFAULT (0),
    IsActive     BIT NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT (1),
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.UserRoles
(
    UserId          INT NOT NULL,
    RoleId          INT NOT NULL,
    AssignedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_UserRoles_AssignedAt DEFAULT (SYSUTCDATETIME()),
    AssignedByUserId INT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY CLUSTERED (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId)
        REFERENCES dbo.Users (Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId)
        REFERENCES dbo.Roles (Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_AssignedBy FOREIGN KEY (AssignedByUserId)
        REFERENCES dbo.Users (Id)
);
GO

/* 5. Shift configuration and employee shift assignments */
CREATE TABLE dbo.ShiftMaster
(
    ShiftId                    INT IDENTITY(1,1) NOT NULL,
    ShiftCode                  VARCHAR(20) NOT NULL,
    ShiftName                  NVARCHAR(100) NOT NULL,
    StartTime                  TIME(0) NOT NULL,
    EndTime                    TIME(0) NOT NULL,
    RequiredWorkMinutes        SMALLINT NOT NULL,
    PlannedBreakMinutes        SMALLINT NOT NULL CONSTRAINT DF_ShiftMaster_PlannedBreakMinutes DEFAULT (0),
    LateGraceMinutes           SMALLINT NOT NULL CONSTRAINT DF_ShiftMaster_LateGraceMinutes DEFAULT (0),
    EarlyDepartureGraceMinutes SMALLINT NOT NULL CONSTRAINT DF_ShiftMaster_EarlyGraceMinutes DEFAULT (0),
    IsActive                   BIT NOT NULL CONSTRAINT DF_ShiftMaster_IsActive DEFAULT (1),
    CONSTRAINT PK_ShiftMaster PRIMARY KEY CLUSTERED (ShiftId),
    CONSTRAINT UQ_ShiftMaster_ShiftCode UNIQUE (ShiftCode),
    CONSTRAINT CK_ShiftMaster_StartEnd CHECK (StartTime <> EndTime),
    CONSTRAINT CK_ShiftMaster_Minutes CHECK
        (RequiredWorkMinutes > 0 AND RequiredWorkMinutes <= 1440
         AND PlannedBreakMinutes >= 0 AND LateGraceMinutes >= 0 AND EarlyDepartureGraceMinutes >= 0)
);
GO

CREATE TABLE dbo.ShiftWorkDays
(
    ShiftId      INT NOT NULL,
    DayOfWeek    TINYINT NOT NULL, -- ISO numbering: Monday=1 through Sunday=7.
    IsWorkingDay BIT NOT NULL,
    CONSTRAINT PK_ShiftWorkDays PRIMARY KEY CLUSTERED (ShiftId, DayOfWeek),
    CONSTRAINT FK_ShiftWorkDays_ShiftMaster FOREIGN KEY (ShiftId)
        REFERENCES dbo.ShiftMaster (ShiftId) ON DELETE CASCADE,
    CONSTRAINT CK_ShiftWorkDays_DayOfWeek CHECK (DayOfWeek BETWEEN 1 AND 7)
);
GO

CREATE TABLE dbo.EmployeeShiftAssignments
(
    AssignmentId    BIGINT IDENTITY(1,1) NOT NULL,
    EmployeeId      INT NOT NULL,
    ShiftId         INT NOT NULL,
    EffectiveFrom   DATE NOT NULL,
    EffectiveTo     DATE NULL,
    AssignedByUserId INT NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeShiftAssignments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_EmployeeShiftAssignments PRIMARY KEY CLUSTERED (AssignmentId),
    CONSTRAINT FK_EmployeeShiftAssignments_Employee FOREIGN KEY (EmployeeId)
        REFERENCES dbo.EmployeeMaster (RowId),
    CONSTRAINT FK_EmployeeShiftAssignments_Shift FOREIGN KEY (ShiftId)
        REFERENCES dbo.ShiftMaster (ShiftId),
    CONSTRAINT FK_EmployeeShiftAssignments_AssignedBy FOREIGN KEY (AssignedByUserId)
        REFERENCES dbo.Users (Id),
    CONSTRAINT CK_EmployeeShiftAssignments_Dates CHECK
        (EffectiveTo IS NULL OR EffectiveTo >= EffectiveFrom)
);
GO
CREATE INDEX IX_EmployeeShiftAssignments_Employee_Dates
    ON dbo.EmployeeShiftAssignments (EmployeeId, EffectiveFrom, EffectiveTo)
    INCLUDE (ShiftId);
GO

/* 6. Application login/logout history - separate from attendance */
CREATE TABLE dbo.UserSessionHistory
(
    SessionHistoryId BIGINT IDENTITY(1,1) NOT NULL,
    SessionId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_UserSessionHistory_SessionId DEFAULT (NEWID()),
    UserId           INT NOT NULL,
    LoginAt          DATETIME2(0) NOT NULL CONSTRAINT DF_UserSessionHistory_LoginAt DEFAULT (SYSUTCDATETIME()),
    LogoutAt         DATETIME2(0) NULL,
    IpAddress        VARCHAR(45) NULL,
    UserAgent        NVARCHAR(1000) NULL,
    LogoutReason     VARCHAR(30) NULL,
    CONSTRAINT PK_UserSessionHistory PRIMARY KEY CLUSTERED (SessionHistoryId),
    CONSTRAINT UQ_UserSessionHistory_SessionId UNIQUE (SessionId),
    CONSTRAINT FK_UserSessionHistory_Users FOREIGN KEY (UserId)
        REFERENCES dbo.Users (Id),
    CONSTRAINT CK_UserSessionHistory_LogoutTime CHECK
        (LogoutAt IS NULL OR LogoutAt >= LoginAt),
    CONSTRAINT CK_UserSessionHistory_LogoutReason CHECK
        (LogoutReason IS NULL OR LogoutReason IN ('USER', 'TIMEOUT', 'ADMIN', 'TOKEN_EXPIRED', 'UNKNOWN'))
);
GO
CREATE INDEX IX_UserSessionHistory_User_LoginAt
    ON dbo.UserSessionHistory (UserId, LoginAt DESC)
    INCLUDE (LogoutAt, IpAddress);
GO

/* 7. Raw work attendance punches. Keep these as the audit/source-of-truth records. */
CREATE TABLE dbo.AttendancePunches
(
    PunchId          BIGINT IDENTITY(1,1) NOT NULL,
    EmployeeId       INT NOT NULL,
    PunchDateTime    DATETIME2(0) NOT NULL,
    PunchType        VARCHAR(20) NOT NULL, -- IN, OUT, BREAK_START, BREAK_END
    PunchSource      VARCHAR(20) NOT NULL CONSTRAINT DF_AttendancePunches_PunchSource DEFAULT ('WEB'),
    DeviceIdentifier NVARCHAR(100) NULL,
    Latitude         DECIMAL(9,6) NULL,
    Longitude        DECIMAL(9,6) NULL,
    IsManualCorrection BIT NOT NULL CONSTRAINT DF_AttendancePunches_IsManualCorrection DEFAULT (0),
    EnteredByUserId  INT NULL,
    Remarks          NVARCHAR(500) NULL,
    CreatedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_AttendancePunches_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_AttendancePunches PRIMARY KEY CLUSTERED (PunchId),
    CONSTRAINT FK_AttendancePunches_Employee FOREIGN KEY (EmployeeId)
        REFERENCES dbo.EmployeeMaster (RowId),
    CONSTRAINT FK_AttendancePunches_EnteredBy FOREIGN KEY (EnteredByUserId)
        REFERENCES dbo.Users (Id),
    CONSTRAINT CK_AttendancePunches_PunchType CHECK
        (PunchType IN ('IN', 'OUT', 'BREAK_START', 'BREAK_END')),
    CONSTRAINT CK_AttendancePunches_PunchSource CHECK
        (PunchSource IN ('WEB', 'MOBILE', 'BIOMETRIC', 'ADMIN', 'IMPORT')),
    CONSTRAINT CK_AttendancePunches_Location CHECK
        ((Latitude IS NULL AND Longitude IS NULL)
         OR (Latitude IS NOT NULL AND Longitude IS NOT NULL
             AND Latitude BETWEEN -90 AND 90 AND Longitude BETWEEN -180 AND 180))
);
GO
CREATE INDEX IX_AttendancePunches_Employee_Time
    ON dbo.AttendancePunches (EmployeeId, PunchDateTime)
    INCLUDE (PunchType, PunchSource, IsManualCorrection);
GO

/* 8. Calculated daily attendance summary. Recalculate it from validated punch events. */
CREATE TABLE dbo.AttendanceDaily
(
    AttendanceId       BIGINT IDENTITY(1,1) NOT NULL,
    EmployeeId         INT NOT NULL,
    AttendanceDate     DATE NOT NULL, -- Local work date at the employee's work location.
    ShiftId            INT NULL,
    ScheduledStartAt   DATETIME2(0) NULL, -- Store UTC instants after resolving the location's time zone.
    ScheduledEndAt     DATETIME2(0) NULL,
    FirstInAt          DATETIME2(0) NULL,
    LastOutAt          DATETIME2(0) NULL,
    TotalWorkedMinutes INT NOT NULL CONSTRAINT DF_AttendanceDaily_Worked DEFAULT (0),
    TotalBreakMinutes  INT NOT NULL CONSTRAINT DF_AttendanceDaily_Break DEFAULT (0),
    LateMinutes        INT NOT NULL CONSTRAINT DF_AttendanceDaily_Late DEFAULT (0),
    EarlyLeaveMinutes  INT NOT NULL CONSTRAINT DF_AttendanceDaily_Early DEFAULT (0),
    OvertimeMinutes    INT NOT NULL CONSTRAINT DF_AttendanceDaily_Overtime DEFAULT (0),
    AttendanceStatus   VARCHAR(20) NOT NULL CONSTRAINT DF_AttendanceDaily_Status DEFAULT ('PENDING'),
    IsApproved         BIT NOT NULL CONSTRAINT DF_AttendanceDaily_IsApproved DEFAULT (0),
    ApprovedByUserId   INT NULL,
    ApprovedAt         DATETIME2(0) NULL,
    ManagerRemarks     NVARCHAR(1000) NULL,
    CreatedAt          DATETIME2(0) NOT NULL CONSTRAINT DF_AttendanceDaily_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt          DATETIME2(0) NULL,
    CONSTRAINT PK_AttendanceDaily PRIMARY KEY CLUSTERED (AttendanceId),
    CONSTRAINT UQ_AttendanceDaily_Employee_Date UNIQUE (EmployeeId, AttendanceDate),
    CONSTRAINT FK_AttendanceDaily_Employee FOREIGN KEY (EmployeeId)
        REFERENCES dbo.EmployeeMaster (RowId),
    CONSTRAINT FK_AttendanceDaily_Shift FOREIGN KEY (ShiftId)
        REFERENCES dbo.ShiftMaster (ShiftId),
    CONSTRAINT FK_AttendanceDaily_ApprovedBy FOREIGN KEY (ApprovedByUserId)
        REFERENCES dbo.Users (Id),
    CONSTRAINT CK_AttendanceDaily_Status CHECK
        (AttendanceStatus IN ('PENDING', 'PRESENT', 'LATE', 'HALF_DAY', 'ABSENT',
                              'ON_LEAVE', 'HOLIDAY', 'WEEKLY_OFF', 'WORK_FROM_HOME', 'INCOMPLETE')),
    CONSTRAINT CK_AttendanceDaily_NonNegativeMinutes CHECK
        (TotalWorkedMinutes >= 0 AND TotalBreakMinutes >= 0 AND LateMinutes >= 0
         AND EarlyLeaveMinutes >= 0 AND OvertimeMinutes >= 0),
    CONSTRAINT CK_AttendanceDaily_PunchOrder CHECK
        (FirstInAt IS NULL OR LastOutAt IS NULL OR FirstInAt <= LastOutAt),
    CONSTRAINT CK_AttendanceDaily_ScheduleOrder CHECK
        (ScheduledStartAt IS NULL OR ScheduledEndAt IS NULL OR ScheduledStartAt < ScheduledEndAt)
);
GO
CREATE INDEX IX_AttendanceDaily_Date_Status
    ON dbo.AttendanceDaily (AttendanceDate, AttendanceStatus)
    INCLUDE (EmployeeId, ShiftId, FirstInAt, LastOutAt, LateMinutes, TotalWorkedMinutes);
GO

/* 9. Missed-punch correction/regularization workflow */
CREATE TABLE dbo.AttendanceAdjustmentRequests
(
    AdjustmentRequestId BIGINT IDENTITY(1,1) NOT NULL,
    EmployeeId          INT NOT NULL,
    AttendanceDate      DATE NOT NULL,
    RequestedPunchType  VARCHAR(20) NOT NULL,
    RequestedPunchAt    DATETIME2(0) NOT NULL,
    Reason              NVARCHAR(1000) NOT NULL,
    RequestStatus       VARCHAR(20) NOT NULL CONSTRAINT DF_AttendanceAdjustmentRequests_Status DEFAULT ('PENDING'),
    RequestedAt         DATETIME2(0) NOT NULL CONSTRAINT DF_AttendanceAdjustmentRequests_RequestedAt DEFAULT (SYSUTCDATETIME()),
    ReviewedByUserId    INT NULL,
    ReviewedAt          DATETIME2(0) NULL,
    ReviewerComments    NVARCHAR(1000) NULL,
    ApprovedPunchId     BIGINT NULL,
    CONSTRAINT PK_AttendanceAdjustmentRequests PRIMARY KEY CLUSTERED (AdjustmentRequestId),
    CONSTRAINT FK_AttendanceAdjustmentRequests_Employee FOREIGN KEY (EmployeeId)
        REFERENCES dbo.EmployeeMaster (RowId),
    CONSTRAINT FK_AttendanceAdjustmentRequests_ReviewedBy FOREIGN KEY (ReviewedByUserId)
        REFERENCES dbo.Users (Id),
    CONSTRAINT FK_AttendanceAdjustmentRequests_ApprovedPunch FOREIGN KEY (ApprovedPunchId)
        REFERENCES dbo.AttendancePunches (PunchId),
    CONSTRAINT CK_AttendanceAdjustmentRequests_PunchType CHECK
        (RequestedPunchType IN ('IN', 'OUT', 'BREAK_START', 'BREAK_END')),
    CONSTRAINT CK_AttendanceAdjustmentRequests_Status CHECK
        (RequestStatus IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED'))
);
GO
CREATE INDEX IX_AttendanceAdjustmentRequests_Employee_Date_Status
    ON dbo.AttendanceAdjustmentRequests (EmployeeId, AttendanceDate, RequestStatus);
GO

/* 10. Leave management */
CREATE TABLE dbo.LeaveTypes
(
    LeaveTypeId    INT IDENTITY(1,1) NOT NULL,
    LeaveCode      VARCHAR(20) NOT NULL,
    LeaveName      NVARCHAR(100) NOT NULL,
    IsPaid         BIT NOT NULL CONSTRAINT DF_LeaveTypes_IsPaid DEFAULT (1),
    MaxDaysPerYear DECIMAL(5,2) NULL,
    RequiresApproval BIT NOT NULL CONSTRAINT DF_LeaveTypes_RequiresApproval DEFAULT (1),
    IsActive       BIT NOT NULL CONSTRAINT DF_LeaveTypes_IsActive DEFAULT (1),
    CONSTRAINT PK_LeaveTypes PRIMARY KEY CLUSTERED (LeaveTypeId),
    CONSTRAINT UQ_LeaveTypes_Code UNIQUE (LeaveCode),
    CONSTRAINT UQ_LeaveTypes_Name UNIQUE (LeaveName),
    CONSTRAINT CK_LeaveTypes_MaxDays CHECK (MaxDaysPerYear IS NULL OR MaxDaysPerYear >= 0)
);
GO

CREATE TABLE dbo.LeaveRequests
(
    LeaveRequestId  BIGINT IDENTITY(1,1) NOT NULL,
    EmployeeId      INT NOT NULL,
    LeaveTypeId     INT NOT NULL,
    StartDate       DATE NOT NULL,
    EndDate         DATE NOT NULL,
    RequestedDays   DECIMAL(5,2) NOT NULL,
    IsHalfDay       BIT NOT NULL CONSTRAINT DF_LeaveRequests_IsHalfDay DEFAULT (0),
    HalfDayPart     VARCHAR(20) NULL, -- FIRST_HALF / SECOND_HALF when IsHalfDay = 1.
    Reason          NVARCHAR(1000) NULL,
    RequestStatus   VARCHAR(20) NOT NULL CONSTRAINT DF_LeaveRequests_Status DEFAULT ('PENDING'),
    RequestedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_LeaveRequests_RequestedAt DEFAULT (SYSUTCDATETIME()),
    ReviewedByUserId INT NULL,
    ReviewedAt      DATETIME2(0) NULL,
    ReviewComments  NVARCHAR(1000) NULL,
    CONSTRAINT PK_LeaveRequests PRIMARY KEY CLUSTERED (LeaveRequestId),
    CONSTRAINT FK_LeaveRequests_Employee FOREIGN KEY (EmployeeId)
        REFERENCES dbo.EmployeeMaster (RowId),
    CONSTRAINT FK_LeaveRequests_LeaveType FOREIGN KEY (LeaveTypeId)
        REFERENCES dbo.LeaveTypes (LeaveTypeId),
    CONSTRAINT FK_LeaveRequests_ReviewedBy FOREIGN KEY (ReviewedByUserId)
        REFERENCES dbo.Users (Id),
    CONSTRAINT CK_LeaveRequests_Dates CHECK (EndDate >= StartDate),
    CONSTRAINT CK_LeaveRequests_RequestedDays CHECK (RequestedDays > 0),
    CONSTRAINT CK_LeaveRequests_HalfDay CHECK
        ((IsHalfDay = 0 AND HalfDayPart IS NULL)
         OR (IsHalfDay = 1 AND HalfDayPart IS NOT NULL AND HalfDayPart IN ('FIRST_HALF', 'SECOND_HALF'))),
    CONSTRAINT CK_LeaveRequests_Status CHECK
        (RequestStatus IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED'))
);
GO
CREATE INDEX IX_LeaveRequests_Employee_Status_Dates
    ON dbo.LeaveRequests (EmployeeId, RequestStatus, StartDate, EndDate);
GO

/* 11. Holidays by company, country, state, or city. All location IDs NULL = company-wide. */
CREATE TABLE dbo.CompanyHolidays
(
    HolidayId   INT IDENTITY(1,1) NOT NULL,
    HolidayName NVARCHAR(150) NOT NULL,
    HolidayDate DATE NOT NULL,
    CountryId   INT NULL,
    StateId     INT NULL,
    CityId      INT NULL,
    IsOptional  BIT NOT NULL CONSTRAINT DF_CompanyHolidays_IsOptional DEFAULT (0),
    IsActive    BIT NOT NULL CONSTRAINT DF_CompanyHolidays_IsActive DEFAULT (1),
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_CompanyHolidays_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_CompanyHolidays PRIMARY KEY CLUSTERED (HolidayId),
    CONSTRAINT FK_CompanyHolidays_Country FOREIGN KEY (CountryId)
        REFERENCES dbo.Country (RowId),
    CONSTRAINT FK_CompanyHolidays_StateCountry FOREIGN KEY (StateId, CountryId)
        REFERENCES dbo.[State] (RowId, CountryId),
    CONSTRAINT FK_CompanyHolidays_CityState FOREIGN KEY (CityId, StateId)
        REFERENCES dbo.City (RowId, StateId),
    CONSTRAINT CK_CompanyHolidays_LocationHierarchy CHECK
        ((StateId IS NULL OR CountryId IS NOT NULL) AND (CityId IS NULL OR StateId IS NOT NULL))
);
GO
CREATE INDEX IX_CompanyHolidays_Date_Location
    ON dbo.CompanyHolidays (HolidayDate, CountryId, StateId, CityId)
    INCLUDE (HolidayName, IsOptional, IsActive);
GO

/* Suggested starter roles. Review/adjust these for your organization before use. */
INSERT INTO dbo.Roles ([Name], Description, IsSystemRole)
VALUES
    (N'Admin', N'Full system administration', 1),
    (N'HR', N'Employee and leave administration', 1),
    (N'Manager', N'Team attendance and leave approvals', 1),
    (N'Employee', N'Personal attendance and leave self-service', 1);
GO

/* Suggested starter leave types. Customize policy rules in application configuration. */
INSERT INTO dbo.LeaveTypes (LeaveCode, LeaveName, IsPaid, RequiresApproval)
VALUES
    ('CL', N'Casual Leave', 1, 1),
    ('SL', N'Sick Leave', 1, 1),
    ('LWP', N'Leave Without Pay', 0, 1);
GO
