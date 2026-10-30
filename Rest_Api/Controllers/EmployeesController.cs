using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Rest_Api.Filters;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR")]
[AuthorizationFilter(Roles = "Admin,HR")]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;
    public EmployeesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] bool includeDeleted = false)
    {
        var query = _db.EmployeeMasters.AsNoTracking().AsQueryable();
        if (!includeDeleted)
            query = query.Where(e => !e.IsDeleted);

        var items = await query
            .OrderBy(e => e.EmployeeCode)
            .Select(e => new
            {
                e.RowId,
                e.EmployeeCode,
                e.FirstName,
                e.LastName,
                e.EmailAddress,
                e.MobileNumber,
                e.CountryId,
                CountryName = e.Country != null ? e.Country.CountryName : null,
                e.StateId,
                StateName = e.State != null ? e.State.StateName : null,
                e.CityId,
                CityName = e.City != null ? e.City.CityName : null,
                e.DepartmentId,
                DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
                e.DesignationId,
                DesignationName = e.Designation != null ? e.Designation.DesignationName : null,
                e.WorkLocationId,
                WorkLocationName = e.WorkLocation != null ? e.WorkLocation.LocationName : null,
                e.ReportingManagerId,
                DateOfBirth = e.DateOfBirth.ToString("yyyy-MM-dd"),
                DateOfJoinee = e.DateOfJoinee.ToString("yyyy-MM-dd"),
                e.IsActive,
                e.IsDeleted
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        var e = await _db.EmployeeMasters.AsNoTracking().FirstOrDefaultAsync(x => x.RowId == id);
        return e == null ? NotFound() : Ok(e);
    }

    /// <summary>Admin/HR: create employee + login user + role in one step.</summary>
    [HttpPost("provision")]
    public async Task<ActionResult<object>> Provision([FromBody] ProvisionEmployeeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId && r.IsActive);
        if (role == null)
            return BadRequest(new { message = "Invalid role." });

        if (await _db.EmployeeMasters.AnyAsync(e => e.EmailAddress == request.EmailAddress))
            return BadRequest(new { message = "Email already exists." });

        var employeeCode = await NextEmployeeCodeAsync();
        var employee = new EmployeeMaster
        {
            EmployeeCode = employeeCode,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailAddress = request.EmailAddress.Trim(),
            MobileNumber = request.MobileNumber,
            CountryId = request.CountryId,
            StateId = request.StateId,
            CityId = request.CityId,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            WorkLocationId = request.WorkLocationId,
            ReportingManagerId = request.ReportingManagerId,
            DateOfBirth = DateOnly.FromDateTime(request.DateOfBirth),
            DateOfJoinee = DateOnly.FromDateTime(request.DateOfJoinee),
            IsActive = request.IsActive,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow
        };

        _db.EmployeeMasters.Add(employee);
        await _db.SaveChangesAsync();

        var user = new User
        {
            EmployeeId = employee.RowId,
            UserName = employeeCode,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = employee.RowId }, new
        {
            employee.RowId,
            employee.EmployeeCode,
            employee.EmailAddress,
            role = role.Name,
            message = "Employee and login account created. Username is the employee code."
        });
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeMaster>> Create([FromBody] EmployeeMaster model)
    {
        model.RowId = 0;
        model.CreatedDate = DateTime.UtcNow;
        model.IsDeleted = false;
        _db.EmployeeMasters.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.RowId }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeMaster model)
    {
        if (id != model.RowId) return BadRequest();
        var existing = await _db.EmployeeMasters.FindAsync(id);
        if (existing == null || existing.IsDeleted) return NotFound();

        existing.FirstName = model.FirstName;
        existing.LastName = model.LastName;
        existing.EmailAddress = model.EmailAddress;
        existing.MobileNumber = model.MobileNumber;
        existing.DepartmentId = model.DepartmentId;
        existing.DesignationId = model.DesignationId;
        existing.WorkLocationId = model.WorkLocationId;
        existing.ReportingManagerId = model.ReportingManagerId;
        existing.CountryId = model.CountryId;
        existing.StateId = model.StateId;
        existing.CityId = model.CityId;
        existing.DateOfBirth = model.DateOfBirth;
        existing.DateOfJoinee = model.DateOfJoinee;
        existing.DateOfExit = model.DateOfExit;
        existing.IsActive = model.IsActive;
        existing.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.EmployeeMasters.FindAsync(id);
        if (existing == null) return NotFound();
        existing.IsDeleted = true;
        existing.IsActive = false;
        existing.DeletedDate = DateTime.UtcNow;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmployeeId == id);
        if (user != null)
        {
            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> NextEmployeeCodeAsync()
    {
        var codes = await _db.EmployeeMasters.AsNoTracking()
            .Where(e => e.EmployeeCode.StartsWith("NEO"))
            .Select(e => e.EmployeeCode)
            .ToListAsync();

        var max = 0;
        foreach (var code in codes)
        {
            if (code.Length >= 7 && int.TryParse(code[3..], out var n) && n > max)
                max = n;
        }

        return $"NEO{(max + 1):D4}";
    }
}
