using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR,Manager,Employee")]
[Route("api/[controller]")]
public class HolidaysController : ControllerBase
{
    private readonly AppDbContext _db;

    public HolidaysController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? year = null)
    {
        var query = _db.CompanyHolidays.AsNoTracking().AsQueryable();
        if (year.HasValue)
            query = query.Where(h => h.HolidayDate.Year == year.Value);

        var items = await query
            .OrderBy(h => h.HolidayDate)
            .Select(h => new
            {
                h.HolidayId,
                h.HolidayName,
                HolidayDate = h.HolidayDate.ToString("yyyy-MM-dd"),
                h.CountryId,
                CountryName = h.Country != null ? h.Country.CountryName : null,
                h.StateId,
                StateName = h.State != null ? h.State.StateName : null,
                h.CityId,
                CityName = h.City != null ? h.City.CityName : null,
                h.IsOptional,
                h.IsActive,
                h.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyHoliday>> Get(int id)
    {
        var item = await _db.CompanyHolidays.FindAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyHoliday>> Create([FromBody] CompanyHoliday model)
    {
        model.HolidayId = 0;
        model.CreatedAt = DateTime.UtcNow;
        _db.CompanyHolidays.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.HolidayId }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CompanyHoliday model)
    {
        if (id != model.HolidayId) return BadRequest();
        var existing = await _db.CompanyHolidays.FindAsync(id);
        if (existing == null) return NotFound();

        existing.HolidayName = model.HolidayName;
        existing.HolidayDate = model.HolidayDate;
        existing.CountryId = model.CountryId;
        existing.StateId = model.StateId;
        existing.CityId = model.CityId;
        existing.IsOptional = model.IsOptional;
        existing.IsActive = model.IsActive;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.CompanyHolidays.FindAsync(id);
        if (existing == null) return NotFound();
        existing.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
