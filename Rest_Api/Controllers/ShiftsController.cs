using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR")]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ShiftsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var items = await _db.ShiftMasters.AsNoTracking()
            .OrderBy(s => s.ShiftName)
            .Select(s => new
            {
                s.ShiftId,
                s.ShiftCode,
                s.ShiftName,
                StartTime = s.StartTime.ToString("HH:mm"),
                EndTime = s.EndTime.ToString("HH:mm"),
                s.RequiredWorkMinutes,
                s.PlannedBreakMinutes,
                s.LateGraceMinutes,
                s.EarlyDepartureGraceMinutes,
                s.IsActive,
                WorkDays = s.ShiftWorkDays.Select(w => new { w.DayOfWeek, w.IsWorkingDay })
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShiftMaster>> Get(int id)
    {
        var item = await _db.ShiftMasters.Include(s => s.ShiftWorkDays).FirstOrDefaultAsync(s => s.ShiftId == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftMaster>> Create([FromBody] ShiftMaster model)
    {
        model.ShiftId = 0;
        _db.ShiftMasters.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.ShiftId }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShiftMaster model)
    {
        if (id != model.ShiftId) return BadRequest();
        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/workdays")]
    public async Task<IActionResult> UpsertWorkDays(int id, [FromBody] List<ShiftWorkDay> days)
    {
        var shift = await _db.ShiftMasters.Include(s => s.ShiftWorkDays).FirstOrDefaultAsync(s => s.ShiftId == id);
        if (shift == null) return NotFound();

        _db.ShiftWorkDays.RemoveRange(shift.ShiftWorkDays);
        foreach (var day in days)
        {
            day.ShiftId = id;
            _db.ShiftWorkDays.Add(day);
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.ShiftMasters.FindAsync(id);
        if (item == null) return NotFound();
        item.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("assignments")]
    public async Task<ActionResult<IEnumerable<object>>> GetAssignments()
    {
        var items = await _db.EmployeeShiftAssignments.AsNoTracking()
            .OrderByDescending(a => a.EffectiveFrom)
            .Select(a => new
            {
                a.AssignmentId,
                a.EmployeeId,
                EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                a.ShiftId,
                ShiftName = a.Shift.ShiftName,
                EffectiveFrom = a.EffectiveFrom.ToString("yyyy-MM-dd"),
                EffectiveTo = a.EffectiveTo.HasValue ? a.EffectiveTo.Value.ToString("yyyy-MM-dd") : null,
                a.CreatedAt
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("assignments")]
    public async Task<ActionResult<EmployeeShiftAssignment>> Assign([FromBody] EmployeeShiftAssignment model)
    {
        model.AssignmentId = 0;
        model.CreatedAt = DateTime.UtcNow;
        _db.EmployeeShiftAssignments.Add(model);
        await _db.SaveChangesAsync();
        return Ok(model);
    }
}
