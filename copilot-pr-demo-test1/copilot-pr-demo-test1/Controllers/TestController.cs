using copilot_pr_demo_test1.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace copilot_pr_demo_test1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Issue 1: No authorization
    // Issue 2: Synchronous operation
    // Issue 3: No error handling
    // Issue 4: Returns entities instead of DTOs
    [Authorize]
    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _context.Users.ToList();
        return Ok(users);
    }

    // Issue 5: SQL Injection
    // Issue 6: No async
    // Issue 7: Hardcoded connection string
    [HttpGet("search")]
    public IActionResult SearchUsers(string name)
    {
        var connectionString = "Server=localhost;Database=Test;Password=Admin123;";
        var sql = $"SELECT * FROM Users WHERE Name = '{name}'";
        // Execute query...
        return Ok();
    }

    // Issue 8: No validation
    // Issue 9: No null check
    // Issue 10: Wrong status code
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        _context.Users.Remove(user);
        _context.SaveChanges();
        return Ok();
    }
}