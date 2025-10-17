using copilot_pr_demo_test1.Models;
using Microsoft.EntityFrameworkCore;

namespace copilot_pr_demo_test1.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
}