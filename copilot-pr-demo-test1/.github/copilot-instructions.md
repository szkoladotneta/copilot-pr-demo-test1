# Copilot Review Instructions

## Project Context
This is an ASP.NET Core 8 Web API for order management. 

## Review Priorities

### Critical Issues (Must Fix)
1. **Security Vulnerabilities**
   - SQL injection
   - Authentication/authorization bypasses
   - Exposed secrets or credentials
   - XSS vulnerabilities
   - Insecure deserialization

2. **Data Integrity Issues**
   - Missing database transactions
   - Race conditions
   - Data loss scenarios

3. **Breaking Changes**
   - Removed or changed public APIs
   - Database schema changes without migrations

### High Priority Issues (Should Fix)
1. **Performance Problems**
   - N+1 query problems
   - Missing async/await in I/O operations
   - Inefficient algorithms
   - Memory leaks

2. **Error Handling**
   - Unhandled exceptions
   - Missing logging
   - Poor error messages

### Medium Priority (Consider Fixing)
1. **Code Quality**
   - Code duplication
   - Complex methods (>50 lines)
   - Magic numbers
   - Poor naming

## Project Standards

### Required Patterns
- ✅ Use Entity Framework Core for database access (never raw SQL)
- ✅ Always use async/await for I/O operations
- ✅ Use DTOs for API responses (never return entities directly)
- ✅ Implement proper authorization on all endpoints
- ✅ Use ILogger for logging (never Console.WriteLine)
- ✅ Wrap multiple database operations in transactions
- ✅ Validate all user inputs
- ✅ Return appropriate HTTP status codes
- ✅ Use dependency injection

### Forbidden Patterns
- ❌ Raw SQL queries (use EF Core LINQ)
- ❌ Blocking async code (.Result, .Wait())
- ❌ Empty catch blocks
- ❌ Hardcoded connection strings or secrets
- ❌ Returning passwords or sensitive data
- ❌ Public endpoints without authorization
- ❌ DateTime.Now (use DateTime.UtcNow)

## Review Guidelines

### When Reviewing Controllers
- Check for [Authorize] attributes
- Verify input validation
- Ensure DTOs are used (not entities)
- Check async/await usage
- Verify proper HTTP status codes

### When Reviewing Services
- Check for proper transaction usage
- Verify error handling
- Check for N+1 query problems
- Ensure async operations
- Verify logging

### When Reviewing Data Access
- Ensure Entity Framework is used
- Check for proper Include() statements
- Verify AsNoTracking() on read-only queries
- Check for pagination on list queries

## Examples of Good Code

### Good Controller
```csharp
[Authorize]
[HttpGet("{id}")]
public async Task<ActionResult<OrderDto>> GetOrder(int id)
{
    if (id <= 0) return BadRequest();
    
    var order = await _orderService.GetOrderAsync(id);
    return order == null ? NotFound() : Ok(order);
}
```

### Good Service Method
```csharp
public async Task<OrderDto?> GetOrderAsync(int id)
{
    try
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.Id == id)
            .Select(o => new OrderDto { ... })
            .FirstOrDefaultAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting order {OrderId}", id);
        throw;
    }
}
```

## Review Tone
- Be constructive and educational
- Explain WHY something is a problem
- Provide code examples for fixes
- Reference official documentation when applicable
- Use severity levels appropriately
