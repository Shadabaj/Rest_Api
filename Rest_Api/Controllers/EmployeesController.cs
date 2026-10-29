using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR")]
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
                e.DepartmentId,
                DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
                e.DesignationId,
                DesignationName = e.Designation != null ? e.Designation.DesignationName : null,
                e.WorkLocationId,
                WorkLocationName = e.WorkLocation != null ? e.WorkLocation.LocationName : null,
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
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
