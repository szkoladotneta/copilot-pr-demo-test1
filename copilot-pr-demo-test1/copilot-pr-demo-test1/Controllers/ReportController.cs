using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace copilot_pr_demo_test1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IConfiguration _config;

    public ReportController(IConfiguration config)
    {
        _config = config;
    }

    // Intentional violations of our standards
    [HttpGet("sales")]
    public IActionResult GetSalesReport(string startDate, string endDate)
    {
        // VIOLATION: Raw SQL (we require EF Core)
        var connStr = _config.GetConnectionString("DefaultConnection");
        var conn = new SqlConnection(connStr);
        conn.Open();

        // VIOLATION: SQL Injection
        var query = $"SELECT * FROM Sales WHERE Date >= '{startDate}' AND Date <= '{endDate}'";
        var cmd = new SqlCommand(query, conn);

        // VIOLATION: Not async
        var reader = cmd.ExecuteReader();
        var results = new List<object>();

        while (reader.Read())
        {
            results.Add(new
            {
                date = reader["Date"],
                amount = reader["Amount"]
            });
        }

        // VIOLATION: Not disposing resources
        return Ok(results);
    }
}