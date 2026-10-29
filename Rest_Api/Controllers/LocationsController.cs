using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR")]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public LocationsController(AppDbContext db) => _db = db;

    [HttpGet("countries")]
    public async Task<ActionResult<IEnumerable<Country>>> Countries()
        => Ok(await _db.Countries.AsNoTracking().OrderBy(x => x.CountryName).ToListAsync());

    [HttpGet("states")]
    public async Task<ActionResult<IEnumerable<object>>> States([FromQuery] int? countryId = null)
    {
        var query = _db.States.AsNoTracking().AsQueryable();
        if (countryId.HasValue) query = query.Where(s => s.CountryId == countryId.Value);
        return Ok(await query.OrderBy(s => s.StateName)
            .Select(s => new { s.RowId, s.CountryId, s.StateName })
            .ToListAsync());
    }

    [HttpGet("cities")]
    public async Task<ActionResult<IEnumerable<object>>> Cities([FromQuery] int? stateId = null)
    {
        var query = _db.Cities.AsNoTracking().AsQueryable();
        if (stateId.HasValue) query = query.Where(c => c.StateId == stateId.Value);
        return Ok(await query.OrderBy(c => c.CityName)
            .Select(c => new { c.RowId, c.StateId, c.CityName })
            .ToListAsync());
    }

    [HttpGet("work-locations")]
    public async Task<ActionResult<IEnumerable<object>>> WorkLocations()
    {
        var items = await _db.WorkLocations.AsNoTracking()
            .OrderBy(w => w.LocationName)
            .Select(w => new
            {
                w.WorkLocationId,
                w.LocationCode,
                w.LocationName,
                w.AddressLine1,
                w.AddressLine2,
                w.CountryId,
                CountryName = w.Country != null ? w.Country.CountryName : null,
                w.StateId,
                StateName = w.State != null ? w.State.StateName : null,
                w.CityId,
                CityName = w.City != null ? w.City.CityName : null,
                w.TimeZoneId,
                w.IsActive
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("work-locations")]
    public async Task<ActionResult<WorkLocation>> CreateWorkLocation([FromBody] WorkLocation model)
    {
        model.WorkLocationId = 0;
        model.CreatedDate = DateTime.UtcNow;
        _db.WorkLocations.Add(model);
        await _db.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPut("work-locations/{id:int}")]
    public async Task<IActionResult> UpdateWorkLocation(int id, [FromBody] WorkLocation model)
    {
        if (id != model.WorkLocationId) return BadRequest();
        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("work-locations/{id:int}")]
    public async Task<IActionResult> DeleteWorkLocation(int id)
    {
        var item = await _db.WorkLocations.FindAsync(id);
        if (item == null) return NotFound();
        item.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
