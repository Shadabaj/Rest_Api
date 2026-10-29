CREATE OR ALTER PROCEDURE dbo.usp_CreateUserAccount
@FirstName varchar(100),
@LastName varchar(100),
@DateOfBirth datetime,
@DateOfJoinee datetime,
@PassWord nvarchar(1000),
@UserRoleId int,
@EmployeeCode  varchar(100) output
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION

DECLARE @EmployeeId INT,@UserId INT,@RoleId int

SET @EmployeeCode=dbo.fn_GetNextEmployeeCode()

 INSERT INTO dbo.EmployeeMaster
    (
        EmployeeCode,
        FirstName,
        LastName,
        DateOfBirth,
        DateOfJoinee
    )
    VALUES
    (
        @EmployeeCode,
        @FirstName,
        @LastName,
        @DateOfBirth,
        @DateOfJoinee
    );


	set @EmployeeId=CONVERT(int, SCOPE_IDENTITY());

	INSERT INTO dbo.Users
    (
        EmployeeId,
        UserName,
        PasswordHash
    )
    VALUES
    (
        @EmployeeId,
        @EmployeeCode,
        @PassWord
    );

	set @UserId= CAST(SCOPE_IDENTITY() AS INT)

	
	SELECT TOP 1 @RoleId=Id FROM Roles WHERE Id=@UserRoleId

	IF @RoleId is not null
	begin
	 INSERT INTO dbo.UserRoles
        (
            UserId,
            RoleId
        )
        VALUES
        (
            @UserId,
            @RoleId
        );


		end

COMMIT TRANSACTION
END TRY
BEGIN CATCH 



 IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH


END