using DAL;
using DAL.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Rest_Api.Filters;

namespace Rest_Api.Controllers;

[ApiController]
[AuthorizationFilter(Roles = "Admin")]
[Route("api/[controller]")]
[EnableRateLimiting("Fixed")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IUserRepo _userRepo;
    public RolesController(AppDbContext db,IUserRepo userRepo) 
    {
        _db = db;
        _userRepo = userRepo;     
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetAll()
        => Ok(await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Role>> Get(int id)
    {
        var item = await _db.Roles.FindAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Role>> Create([FromBody] Role model)
    {
        model.Id = 0;
        _db.Roles.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Role model)
    {
        if (id != model.Id) return BadRequest();
        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Roles.FindAsync(id);
        if (item == null) return NotFound();
        item.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }




}
