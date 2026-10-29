using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rest_Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR")]
[Route("api/[controller]")]
public class LeaveTypesController : ControllerBase
{
    private readonly AppDbContext _db;
    public LeaveTypesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveType>>> GetAll()
        => Ok(await _db.LeaveTypes.AsNoTracking().OrderBy(x => x.LeaveName).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeaveType>> Get(int id)
    {
        var item = await _db.LeaveTypes.FindAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<LeaveType>> Create([FromBody] LeaveType model)
    {
        model.LeaveTypeId = 0;
        _db.LeaveTypes.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.LeaveTypeId }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LeaveType model)
    {
        if (id != model.LeaveTypeId) return BadRequest();
        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.LeaveTypes.FindAsync(id);
        if (item == null) return NotFound();
        item.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
