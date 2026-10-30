using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Rest_Api.Filters;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TripsController(AppDbContext db) => _db = db;

    private int? UserId()
    {
        var raw = User.FindFirst("userId")?.Value;
        return int.TryParse(raw, out var id) ? id : null;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var items = await _db.TripPlans.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.StartDate)
            .Select(t => new
            {
                t.TripPlanId,
                t.Title,
                t.Description,
                StartDate = t.StartDate.ToString("yyyy-MM-dd"),
                EndDate = t.EndDate.ToString("yyyy-MM-dd"),
                t.TargetAmount,
                t.CollectedAmount,
                Remaining = t.TargetAmount - t.CollectedAmount,
                Contributors = t.TripPayments.Count
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    [AuthorizationFilter(Roles = "Admin,HR")]
    public async Task<ActionResult<object>> Create([FromBody] TripPlanRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var trip = new TripPlan
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            StartDate = DateOnly.FromDateTime(request.StartDate),
            EndDate = DateOnly.FromDateTime(request.EndDate),
            TargetAmount = request.TargetAmount,
            CollectedAmount = 0,
            CreatedByUserId = UserId(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.TripPlans.Add(trip);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = trip.TripPlanId }, trip);
    }

    /// <summary>Logged-in user pays toward a trip plan (simulated payment).</summary>
    [HttpPost("{id:int}/pay")]
    public async Task<ActionResult<object>> Pay(int id, [FromBody] TripPaymentRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userId = UserId();
        if (userId == null) return Unauthorized();

        var trip = await _db.TripPlans.FirstOrDefaultAsync(t => t.TripPlanId == id && t.IsActive);
        if (trip == null) return NotFound(new { message = "Trip not found." });

        if (request.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        var remaining = trip.TargetAmount - trip.CollectedAmount;
        if (request.Amount > remaining)
            return BadRequest(new { message = $"Amount exceeds remaining balance ({remaining:0.00})." });

        var payment = new TripPayment
        {
            TripPlanId = trip.TripPlanId,
            UserId = userId.Value,
            Amount = request.Amount,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "UPI" : request.PaymentMethod,
            PaymentStatus = "PAID",
            PaidAt = DateTime.UtcNow,
            ReferenceCode = $"TRIP-{trip.TripPlanId}-{Guid.NewGuid():N}"[..20].ToUpperInvariant()
        };

        trip.CollectedAmount += request.Amount;
        _db.TripPayments.Add(payment);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Payment successful.",
            payment.ReferenceCode,
            payment.Amount,
            trip.CollectedAmount,
            remaining = trip.TargetAmount - trip.CollectedAmount
        });
    }

    [HttpGet("{id:int}/payments")]
    public async Task<ActionResult<IEnumerable<object>>> Payments(int id)
    {
        var items = await _db.TripPayments.AsNoTracking()
            .Where(p => p.TripPlanId == id)
            .OrderByDescending(p => p.PaidAt)
            .Select(p => new
            {
                p.TripPaymentId,
                p.Amount,
                p.PaymentMethod,
                p.PaymentStatus,
                p.ReferenceCode,
                p.PaidAt,
                UserName = p.User.UserName
            })
            .ToListAsync();
        return Ok(items);
    }
}
