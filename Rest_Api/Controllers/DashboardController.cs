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
    public async Task<ActionResult<object>> Summary()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = new
        {
            employees = await _db.EmployeeMasters.CountAsync(e => !e.IsDeleted && e.IsActive),
            departments = await _db.Departments.CountAsync(d => d.IsActive),
            pendingLeaves = await _db.LeaveRequests.CountAsync(l => l.RequestStatus == "PENDING"),
            pendingAdjustments = await _db.AttendanceAdjustmentRequests.CountAsync(a => a.RequestStatus == "PENDING"),
            holidaysThisYear = await _db.CompanyHolidays.CountAsync(h => h.IsActive && h.HolidayDate.Year == today.Year),
            presentToday = await _db.AttendanceDailies.CountAsync(a =>
                a.AttendanceDate == today &&
                (a.AttendanceStatus == "PRESENT" || a.AttendanceStatus == "Present"))
        };
        return Ok(summary);
    }
}
