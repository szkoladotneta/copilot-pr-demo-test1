# Copilot Review Instructions

## Project Context
ASP.NET Core 9 Web API for order management

## Critical Issues (Block PR) - start comment with "🔴 Critical vulnerability"
- SQL injection vulnerabilities
- Missing [Authorize] attributes
- Storing passwords/secrets in code

## High Priority Issues (Warn)  - start comment with "🟠 Warning"
- Missing async/await on I/O operations
- No input validation
- Missing error handling

## Our Standards  - start comment with "🔵 Code quality issue"
- Use Entity Framework Core (not raw SQL)
- All endpoints require authorization
- Return DTOs, not entities
