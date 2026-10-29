using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR,Manager")]
[Route("api/[controller]")]
public class LeaveRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    public LeaveRequestsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] string? status = null)
    {
        var query = _db.LeaveRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.RequestStatus == status);

        var items = await query
            .OrderByDescending(x => x.RequestedAt)
            .Select(x => new
            {
                x.LeaveRequestId,
                x.EmployeeId,
                EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName,
                EmployeeCode = x.Employee.EmployeeCode,
                x.LeaveTypeId,
                LeaveTypeName = x.LeaveType.LeaveName,
                StartDate = x.StartDate.ToString("yyyy-MM-dd"),
                EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                x.RequestedDays,
                x.IsHalfDay,
                x.HalfDayPart,
                x.Reason,
                x.RequestStatus,
                x.RequestedAt,
                x.ReviewedAt,
                x.ReviewComments
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<LeaveRequest>> Get(long id)
    {
        var item = await _db.LeaveRequests.FindAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequest>> Create([FromBody] LeaveRequest model)
    {
        model.LeaveRequestId = 0;
        model.RequestStatus = string.IsNullOrWhiteSpace(model.RequestStatus) ? "PENDING" : model.RequestStatus;
        model.RequestedAt = DateTime.UtcNow;
        _db.LeaveRequests.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.LeaveRequestId }, model);
    }

    [HttpPut("{id:long}/review")]
    public async Task<IActionResult> Review(long id, [FromBody] ReviewRequest body)
    {
        var item = await _db.LeaveRequests.FindAsync(id);
        if (item == null) return NotFound();
        item.RequestStatus = body.Status;
        item.ReviewComments = body.Comments;
        item.ReviewedAt = DateTime.UtcNow;
        item.ReviewedByUserId = body.ReviewedByUserId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await _db.LeaveRequests.FindAsync(id);
        if (item == null) return NotFound();
        _db.LeaveRequests.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    public class ReviewRequest
    {
        public string Status { get; set; } = "APPROVED";
        public string? Comments { get; set; }
        public int? ReviewedByUserId { get; set; }
    }
}
