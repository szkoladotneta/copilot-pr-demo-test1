# Copilot Code Review Instructions

## Project Context

**Project:** ASP.NET Core 8 Web API Demo  
**Purpose:** Demonstration of automated code reviews  
**Architecture:** RESTful API with MVC pattern  
**Target Framework:** .NET 8.0

---

## Critical Issues (🔴 Block PR - Must Fix)

### Security Vulnerabilities

**SQL Injection:**
```csharp
// ❌ CRITICAL - Never concatenate user input into SQL
var sql = "SELECT * FROM Users WHERE Id = '" + userId + "'";

// ✅ CORRECT - Use parameterized queries or EF Core
var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
```

**Missing Authorization:**
```csharp
// ❌ CRITICAL - Public endpoints without authorization
[HttpPost("admin/delete-all")]
public IActionResult DeleteAll() { }

// ✅ CORRECT - Always require authorization
[Authorize(Roles = "Admin")]
[HttpPost("admin/delete-all")]
public IActionResult DeleteAll() { }
```

**Hardcoded Secrets:**
```csharp
// ❌ CRITICAL - Never hardcode credentials
var connectionString = "Server=prod;Password=Admin123;";

// ✅ CORRECT - Use configuration
var connectionString = _configuration.GetConnectionString("DefaultConnection");
```

**Password Storage:**
```csharp
// ❌ CRITICAL - Never return passwords in API responses
public class UserDto 
{
    public string Password { get; set; } // NEVER!
}

// ✅ CORRECT - Exclude sensitive data
public class UserDto 
{
    public int Id { get; set; }
    public string Username { get; set; }
    // No password field
}
```

---

## High Priority Issues (🟠 Should Fix)

### Async/Await
```csharp
// ❌ Wrong - Blocking I/O operations
[HttpGet]
public IActionResult GetUsers()
{
    var users = _context.Users.ToList(); // Blocks thread
    return Ok(users);
}

// ✅ Correct - Async all the way
[HttpGet]
public async Task<ActionResult<List<UserDto>>> GetUsers()
{
    var users = await _context.Users.ToListAsync();
    return Ok(users);
}
```

### Error Handling
```csharp
// ❌ Wrong - No error handling
[HttpGet("{id}")]
public async Task<ActionResult<User>> GetUser(int id)
{
    var user = await _context.Users.FindAsync(id);
    return Ok(user); // Returns null without checking!
}

// ✅ Correct - Proper error handling
[HttpGet("{id}")]
public async Task<ActionResult<UserDto>> GetUser(int id)
{
    if (id <= 0)
        return BadRequest("Invalid user ID");
    
    var user = await _context.Users.FindAsync(id);
    
    if (user == null)
        return NotFound($"User {id} not found");
    
    return Ok(MapToDto(user));
}
```

### Input Validation
```csharp
// ❌ Wrong - No validation
[HttpPost]
public async Task<IActionResult> CreateUser(User user)
{
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return Ok();
}

// ✅ Correct - Validate inputs
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // Additional validation
    if (string.IsNullOrWhiteSpace(dto.Email))
        return BadRequest("Email is required");
    
    // Process...
}
```

---

## Medium Priority Issues (🟡 Consider Fixing)

### Code Organization

- Methods longer than 50 lines should be refactored
- Classes with more than 10 public methods need review
- Nested if statements deeper than 3 levels should be simplified

### Documentation
```csharp
// ❌ Missing documentation
public async Task<User> GetUser(int id)

// ✅ Documented
/// <summary>
/// Retrieves a user by their unique identifier.
/// </summary>
/// <param name="id">The user's unique ID.</param>
/// <returns>The user if found, null otherwise.</returns>
public async Task<User> GetUser(int id)
```

### Magic Numbers
```csharp
// ❌ Magic numbers
if (order.Total > 1000) { }

// ✅ Named constants
private const decimal LargeOrderThreshold = 1000m;
if (order.Total > LargeOrderThreshold) { }
```

---

## Project Standards

### Our Technology Stack

**Required:**
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- Built-in dependency injection
- System.Text.Json (not Newtonsoft.Json)

**Forbidden:**
- Raw SQL queries (use EF Core)
- Synchronous database operations
- Console.WriteLine (use ILogger)
- Empty catch blocks

### Naming Conventions
```csharp
// Controllers
public class UserController : ControllerBase { }  // ✅
public class userController : ControllerBase { }  // ❌

// DTOs
public class UserDto { }           // ✅
public class UserModel { }         // ❌ (we don't use "Model" suffix)

// Services/Interfaces
public interface IUserService { }  // ✅
public interface UserService { }   // ❌ (missing 'I' prefix)
```

### HTTP Status Codes

Use appropriate status codes:
- `200 OK` - Successful GET/PUT
- `201 Created` - Successful POST with new resource
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not authorized
- `404 Not Found` - Resource doesn't exist
- `500 Internal Server Error` - Unexpected errors

---

## Review Examples

### Example 1: Security Issue

**If you see this:**
```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    var user = await _context.Users.FindAsync(id);
    _context.Users.Remove(user);
    await _context.SaveChangesAsync();
    return Ok();
}
```

**Comment:**
```
🔴 Critical: Missing Authorization (Line 1)

This endpoint allows anyone to delete any user without authorization.

Issues:
1. No [Authorize] attribute
2. No ownership verification
3. No audit logging

Fix:
[Authorize]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    // Verify user can only delete themselves, or is admin
    if (id != GetCurrentUserId() && !User.IsInRole("Admin"))
        return Forbid();
    
    var user = await _context.Users.FindAsync(id);
    if (user == null)
        return NotFound();
    
    _context.Users.Remove(user);
    await _context.SaveChangesAsync();
    
    _logger.LogWarning("User {UserId} deleted by {ActorId}", id, GetCurrentUserId());
    
    return NoContent();
}
```

### Example 2: Performance Issue

**If you see this:**
```csharp
[HttpGet]
public async Task<IActionResult> GetOrders()
{
    var orders = await _context.Orders.ToListAsync();
    
    foreach (var order in orders)
    {
        order.Items = await _context.OrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync();
    }
    
    return Ok(orders);
}
```

**Comment:**
```
🟠 Performance: N+1 Query Problem (Lines 5-9)

This code executes one query per order to load items. With 100 orders, 
this becomes 101 database queries.

Impact:
- 100 orders = ~1-2 seconds
- 1000 orders = timeout
- High database load

Fix:
[HttpGet]
public async Task<IActionResult> GetOrders()
{
    var orders = await _context.Orders
        .Include(o => o.Items)  // Load items in one query
        .ToListAsync();
    
    return Ok(orders);
}

This reduces to a single query with a SQL JOIN.
```

---

## Review Tone

When reviewing code:

✅ **Do:**
- Explain WHY something is a problem
- Provide code examples for fixes
- Reference security standards (OWASP, etc.)
- Acknowledge good patterns
- Be educational and constructive

❌ **Don't:**
- Just say "this is wrong" without explanation
- Overwhelm with minor issues if critical ones exist
- Be condescending or harsh
- Flag things that follow our documented standards

---

## When to Block vs. Warn

**Block PR (🔴 Critical):**
- Security vulnerabilities
- Data loss risks
- Authentication/authorization missing
- Hardcoded secrets
- Exposing sensitive data

**Warn but allow (🟠 High):**
- Performance issues (N+1 queries)
- Missing error handling
- Synchronous I/O
- Missing validation

**Suggest (🟡 Medium):**
- Code style inconsistencies
- Missing documentation
- Complex methods
- Code duplication

---

## Checklist for Every Review

For each PR, verify:

- [ ] Are all endpoints properly authorized?
- [ ] Is user input validated?
- [ ] Are database operations async?
- [ ] Is error handling present?
- [ ] Are appropriate HTTP status codes used?
- [ ] Is sensitive data excluded from responses?
- [ ] Are there any obvious security vulnerabilities?
- [ ] Does the code follow our naming conventions?
- [ ] Is the code reasonably documented?

---

## Special Cases

### WeatherForecast Example Code

The default WeatherForecastController from the template can be ignored or 
flagged as "demo code to be removed." Focus on actual implementation code.

### appsettings.json

If connection strings contain "localhost" or "127.0.0.1", that's acceptable 
for development. Production settings should use environment variables or 
Azure Key Vault.

---

Last Updated: 2025-01-19  
Version: 1.0
