using System.Security.Claims;
using DAL;
using DAL.Entity;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Rest_Api.HangfireJobs;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Employee,Manager,HR,Admin")]
[Route("api/[controller]")]
public class MeController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IBackgroundJobClient _jobs;

    public MeController(AppDbContext db, IBackgroundJobClient jobs)
    {
        _db = db;
        _jobs = jobs;
    }

    private int? GetEmployeeId()
    {
        var claim = User.FindFirst("employeeId")?.Value
                    ?? User.FindFirst("EmployeeId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    private int? GetUserId()
    {
        var claim = User.FindFirst("userId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<object>> Profile()
    {
        var employeeId = GetEmployeeId();
        if (employeeId == null) return Unauthorized(new { message = "Employee claim missing." });

        var e = await _db.EmployeeMasters.AsNoTracking()
            .Where(x => x.RowId == employeeId && !x.IsDeleted)
            .Select(x => new
            {
                x.RowId,
                x.EmployeeCode,
                x.FirstName,
                x.LastName,
                x.EmailAddress,
                x.MobileNumber,
                DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                DesignationName = x.Designation != null ? x.Designation.DesignationName : null,
                WorkLocationName = x.WorkLocation != null ? x.WorkLocation.LocationName : null,
                DateOfJoinee = x.DateOfJoinee.ToString("yyyy-MM-dd"),
                x.IsActive
            })
            .FirstOrDefaultAsync();

        return e == null ? NotFound() : Ok(e);
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<IEnumerable<object>>> MyAttendance()
    {
        var employeeId = GetEmployeeId();
        if (employeeId == null) return Unauthorized(new { message = "Employee claim missing." });

        var items = await _db.AttendanceDailies.AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.AttendanceDate)
            .Take(60)
            .Select(x => new
            {
                AttendanceDate = x.AttendanceDate.ToString("yyyy-MM-dd"),
                ShiftName = x.Shift != null ? x.Shift.ShiftName : null,
                x.FirstInAt,
                x.LastOutAt,
                x.TotalWorkedMinutes,
                x.LateMinutes,
                x.AttendanceStatus
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("punches")]
    public async Task<ActionResult<IEnumerable<object>>> MyPunches()
    {
        var employeeId = GetEmployeeId();
        if (employeeId == null) return Unauthorized(new { message = "Employee claim missing." });

        var items = await _db.AttendancePunches.AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.PunchDateTime)
            .Take(100)
            .Select(x => new
            {
                x.PunchDateTime,
                x.PunchType,
                x.PunchSource,
                x.Remarks
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("leaves")]
    public async Task<ActionResult<IEnumerable<object>>> MyLeaves()
    {
        var employeeId = GetEmployeeId();
        if (employeeId == null) return Unauthorized(new { message = "Employee claim missing." });

        var items = await _db.LeaveRequests.AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.RequestedAt)
            .Select(x => new
            {
                x.LeaveRequestId,
                LeaveTypeName = x.LeaveType.LeaveName,
                StartDate = x.StartDate.ToString("yyyy-MM-dd"),
                EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                x.RequestedDays,
                x.IsHalfDay,
                x.Reason,
                x.RequestStatus,
                x.RequestedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("leaves")]
    public async Task<ActionResult<object>> SubmitLeave([FromBody] SubmitLeaveRequest request)
    {
        var employeeId = GetEmployeeId();
        if (employeeId == null) return Unauthorized(new { message = "Employee claim missing." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var start = DateOnly.FromDateTime(request.StartDate);
        var end = DateOnly.FromDateTime(request.EndDate);
        if (end < start) return BadRequest(new { message = "End date must be on or after start date." });

        var leaveType = await _db.LeaveTypes.FirstOrDefaultAsync(t => t.LeaveTypeId == request.LeaveTypeId && t.IsActive);
        if (leaveType == null) return BadRequest(new { message = "Invalid leave type." });

        var days = request.IsHalfDay
            ? 0.5m
            : Math.Max(1, end.DayNumber - start.DayNumber + 1);

        var model = new LeaveRequest
        {
            EmployeeId = employeeId.Value,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = start,
            EndDate = end,
            RequestedDays = days,
            IsHalfDay = request.IsHalfDay,
            HalfDayPart = request.HalfDayPart,
            Reason = request.Reason,
            RequestStatus = "PENDING",
            RequestedAt = DateTime.UtcNow
        };

        _db.LeaveRequests.Add(model);
        await _db.SaveChangesAsync();

        _jobs.Enqueue<NotifyManagerLeaveJob>(j => j.Execute(model.LeaveRequestId));

        return Ok(new
        {
            message = "Leave submitted. Your manager will be notified by email.",
            model.LeaveRequestId,
            model.RequestedDays,
            model.RequestStatus
        });
    }

    [HttpGet("attendance/month")]
    public async Task<ActionResult<object>> MyAttendanceMonth([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var employeeId = GetEmployeeId();
        if (employeeId == null) return Unauthorized(new { message = "Employee claim missing." });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var y = year ?? today.Year;
        var m = month ?? today.Month;
        var start = new DateOnly(y, m, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var rows = await _db.AttendanceDailies.AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.AttendanceDate >= start && a.AttendanceDate <= end)
            .OrderBy(a => a.AttendanceDate)
            .Select(a => new
            {
                date = a.AttendanceDate.ToString("yyyy-MM-dd"),
                a.AttendanceStatus,
                a.TotalWorkedMinutes
            })
            .ToListAsync();

        var presentDays = rows.Count(r =>
            r.AttendanceStatus != null &&
            (r.AttendanceStatus.Equals("PRESENT", StringComparison.OrdinalIgnoreCase)
             || r.AttendanceStatus.Equals("Present", StringComparison.OrdinalIgnoreCase)
             || r.AttendanceStatus.Equals("WFH", StringComparison.OrdinalIgnoreCase)));

        return Ok(new
        {
            year = y,
            month = m,
            label = start.ToString("MMMM yyyy"),
            presentDays,
            totalRecords = rows.Count,
            days = rows
        });
    }

    [HttpGet("clock-status")]
    public async Task<ActionResult<object>> ClockStatus()
    {
        var employeeId = GetEmployeeId();
        if (employeeId == null) return Unauthorized(new { message = "Employee claim missing." });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var lastPunch = await _db.AttendancePunches.AsNoTracking()
            .Where(p => p.EmployeeId == employeeId && p.PunchDateTime >= start && p.PunchDateTime < end)
            .OrderByDescending(p => p.PunchDateTime)
            .FirstOrDefaultAsync();

        var isClockedIn = lastPunch != null &&
                          string.Equals(lastPunch.PunchType, "IN", StringComparison.OrdinalIgnoreCase);

        return Ok(new
        {
            serverTime = DateTime.UtcNow,
            isClockedIn,
            lastPunchType = lastPunch?.PunchType,
            lastPunchAt = lastPunch?.PunchDateTime
        });
    }

    [HttpPost("clock-in")]
    public async Task<ActionResult<object>> ClockIn()
    {
        var employeeId = GetEmployeeId();
        if (employeeId == null) return Unauthorized(new { message = "Employee claim missing." });

        var status = await GetTodayClockState(employeeId.Value);
        if (status.IsClockedIn)
            return BadRequest(new { message = "You are already clocked in." });

        var now = DateTime.UtcNow;
        var punch = new AttendancePunch
        {
            EmployeeId = employeeId.Value,
            PunchDateTime = now,
            PunchType = "IN",
            PunchSource = "WEB",
            IsManualCorrection = false,
            EnteredByUserId = GetUserId(),
            Remarks = "Clock in",
            CreatedAt = now
        };
        _db.AttendancePunches.Add(punch);
        await UpsertDailyOnClockAsync(employeeId.Value, now, isIn: true);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Clocked in successfully.",
            punch.PunchId,
            punch.PunchType,
            punch.PunchDateTime
        });
    }

    [HttpPost("clock-out")]
    public async Task<ActionResult<object>> ClockOut()
    {
        var employeeId = GetEmployeeId();
        if (employeeId == null) return Unauthorized(new { message = "Employee claim missing." });

        var status = await GetTodayClockState(employeeId.Value);
        if (!status.IsClockedIn)
            return BadRequest(new { message = "You are not clocked in." });

        var now = DateTime.UtcNow;
        var punch = new AttendancePunch
        {
            EmployeeId = employeeId.Value,
            PunchDateTime = now,
            PunchType = "OUT",
            PunchSource = "WEB",
            IsManualCorrection = false,
            EnteredByUserId = GetUserId(),
            Remarks = "Clock out",
            CreatedAt = now
        };
        _db.AttendancePunches.Add(punch);
        await UpsertDailyOnClockAsync(employeeId.Value, now, isIn: false);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Clocked out successfully.",
            punch.PunchId,
            punch.PunchType,
            punch.PunchDateTime
        });
    }

    private async Task<(bool IsClockedIn, AttendancePunch? LastPunch)> GetTodayClockState(int employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var lastPunch = await _db.AttendancePunches
            .Where(p => p.EmployeeId == employeeId && p.PunchDateTime >= start && p.PunchDateTime < end)
            .OrderByDescending(p => p.PunchDateTime)
            .FirstOrDefaultAsync();

        var isClockedIn = lastPunch != null &&
                          string.Equals(lastPunch.PunchType, "IN", StringComparison.OrdinalIgnoreCase);
        return (isClockedIn, lastPunch);
    }

    private async Task UpsertDailyOnClockAsync(int employeeId, DateTime punchAt, bool isIn)
    {
        var day = DateOnly.FromDateTime(punchAt);
        var daily = await _db.AttendanceDailies
            .FirstOrDefaultAsync(d => d.EmployeeId == employeeId && d.AttendanceDate == day);

        if (daily == null)
        {
            daily = new AttendanceDaily
            {
                EmployeeId = employeeId,
                AttendanceDate = day,
                TotalWorkedMinutes = 0,
                TotalBreakMinutes = 0,
                LateMinutes = 0,
                EarlyLeaveMinutes = 0,
                OvertimeMinutes = 0,
                AttendanceStatus = "PRESENT",
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.AttendanceDailies.Add(daily);
        }

        if (isIn)
        {
            daily.FirstInAt ??= punchAt;
            daily.AttendanceStatus = "PRESENT";
        }
        else
        {
            daily.LastOutAt = punchAt;
            if (daily.FirstInAt.HasValue)
            {
                var worked = (int)Math.Max(0, (punchAt - daily.FirstInAt.Value).TotalMinutes);
                daily.TotalWorkedMinutes = worked;
            }
        }

        daily.UpdatedAt = DateTime.UtcNow;
    }
}
