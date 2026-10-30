using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR,Manager")]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<ActionResult<object>> Summary([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var y = year ?? today.Year;
        var m = month ?? today.Month;
        var monthStart = new DateOnly(y, m, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var monthRows = await _db.AttendanceDailies.AsNoTracking()
            .Where(a => a.AttendanceDate >= monthStart && a.AttendanceDate <= monthEnd)
            .Select(a => a.AttendanceStatus)
            .ToListAsync();

        bool IsPresent(string? s) =>
            !string.IsNullOrWhiteSpace(s) &&
            (s.Equals("PRESENT", StringComparison.OrdinalIgnoreCase)
             || s.Equals("Present", StringComparison.OrdinalIgnoreCase)
             || s.Equals("WFH", StringComparison.OrdinalIgnoreCase));

        bool IsLeave(string? s) =>
            !string.IsNullOrWhiteSpace(s) &&
            s.Contains("LEAVE", StringComparison.OrdinalIgnoreCase);

        var presentToday = await _db.AttendanceDailies.CountAsync(a =>
            a.AttendanceDate == today &&
            (a.AttendanceStatus == "PRESENT" || a.AttendanceStatus == "Present" || a.AttendanceStatus == "WFH"));

        var summary = new
        {
            employees = await _db.EmployeeMasters.CountAsync(e => !e.IsDeleted && e.IsActive),
            departments = await _db.Departments.CountAsync(d => d.IsActive),
            pendingLeaves = await _db.LeaveRequests.CountAsync(l => l.RequestStatus == "PENDING"),
            pendingAdjustments = await _db.AttendanceAdjustmentRequests.CountAsync(a => a.RequestStatus == "PENDING"),
            holidaysThisYear = await _db.CompanyHolidays.CountAsync(h => h.IsActive && h.HolidayDate.Year == today.Year),
            presentToday,
            month = new { year = y, month = m, label = monthStart.ToString("MMMM yyyy") },
            presentDaysThisMonth = monthRows.Count(IsPresent),
            leaveDaysThisMonth = monthRows.Count(IsLeave),
            absentDaysThisMonth = monthRows.Count(s =>
                !string.IsNullOrWhiteSpace(s) &&
                s.Equals("ABSENT", StringComparison.OrdinalIgnoreCase)),
            attendanceRecordsThisMonth = monthRows.Count,
            activeTrips = await _db.TripPlans.CountAsync(t => t.IsActive && t.EndDate >= today)
        };
        return Ok(summary);
    }
}
