using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR,Manager")]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _db;
    public AttendanceController(AppDbContext db) => _db = db;

    [HttpGet("daily")]
    public async Task<ActionResult<IEnumerable<object>>> GetDaily(
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] int? employeeId = null)
    {
        var query = _db.AttendanceDailies.AsNoTracking().AsQueryable();
        if (from.HasValue) query = query.Where(x => x.AttendanceDate >= from.Value);
        if (to.HasValue) query = query.Where(x => x.AttendanceDate <= to.Value);
        if (employeeId.HasValue) query = query.Where(x => x.EmployeeId == employeeId.Value);

        var items = await query
            .OrderByDescending(x => x.AttendanceDate)
            .Select(x => new
            {
                x.AttendanceId,
                x.EmployeeId,
                EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName,
                AttendanceDate = x.AttendanceDate.ToString("yyyy-MM-dd"),
                x.ShiftId,
                ShiftName = x.Shift != null ? x.Shift.ShiftName : null,
                x.FirstInAt,
                x.LastOutAt,
                x.TotalWorkedMinutes,
                x.LateMinutes,
                x.EarlyLeaveMinutes,
                x.OvertimeMinutes,
                x.AttendanceStatus,
                x.IsApproved
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("punches")]
    public async Task<ActionResult<IEnumerable<object>>> GetPunches(
        [FromQuery] int? employeeId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var query = _db.AttendancePunches.AsNoTracking().AsQueryable();
        if (employeeId.HasValue) query = query.Where(x => x.EmployeeId == employeeId.Value);
        if (from.HasValue) query = query.Where(x => x.PunchDateTime >= from.Value);
        if (to.HasValue) query = query.Where(x => x.PunchDateTime <= to.Value);

        var items = await query
            .OrderByDescending(x => x.PunchDateTime)
            .Select(x => new
            {
                x.PunchId,
                x.EmployeeId,
                EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName,
                x.PunchDateTime,
                x.PunchType,
                x.PunchSource,
                x.IsManualCorrection,
                x.Remarks
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("punches")]
    public async Task<ActionResult<AttendancePunch>> CreatePunch([FromBody] AttendancePunch model)
    {
        model.PunchId = 0;
        model.CreatedAt = DateTime.UtcNow;
        _db.AttendancePunches.Add(model);
        await _db.SaveChangesAsync();
        return Ok(model);
    }

    [HttpGet("adjustments")]
    public async Task<ActionResult<IEnumerable<object>>> GetAdjustments([FromQuery] string? status = null)
    {
        var query = _db.AttendanceAdjustmentRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.RequestStatus == status);

        var items = await query
            .OrderByDescending(x => x.RequestedAt)
            .Select(x => new
            {
                x.AdjustmentRequestId,
                x.EmployeeId,
                EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName,
                AttendanceDate = x.AttendanceDate.ToString("yyyy-MM-dd"),
                x.RequestedPunchType,
                x.RequestedPunchAt,
                x.Reason,
                x.RequestStatus,
                x.RequestedAt,
                x.ReviewerComments
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("adjustments")]
    public async Task<ActionResult<AttendanceAdjustmentRequest>> CreateAdjustment([FromBody] AttendanceAdjustmentRequest model)
    {
        model.AdjustmentRequestId = 0;
        model.RequestStatus = string.IsNullOrWhiteSpace(model.RequestStatus) ? "PENDING" : model.RequestStatus;
        model.RequestedAt = DateTime.UtcNow;
        _db.AttendanceAdjustmentRequests.Add(model);
        await _db.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPut("adjustments/{id:long}/review")]
    public async Task<IActionResult> ReviewAdjustment(long id, [FromBody] ReviewBody body)
    {
        var item = await _db.AttendanceAdjustmentRequests.FindAsync(id);
        if (item == null) return NotFound();
        item.RequestStatus = body.Status;
        item.ReviewerComments = body.Comments;
        item.ReviewedAt = DateTime.UtcNow;
        item.ReviewedByUserId = body.ReviewedByUserId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    public class ReviewBody
    {
        public string Status { get; set; } = "APPROVED";
        public string? Comments { get; set; }
        public int? ReviewedByUserId { get; set; }
    }
}
