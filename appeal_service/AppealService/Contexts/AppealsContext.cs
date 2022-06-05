using AppealService.Models;
using Microsoft.EntityFrameworkCore;

namespace AppealService.Contexts;

public sealed class AppealsContext : DbContext
{
    public AppealsContext(DbContextOptions<AppealsContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    public DbSet<Appeal> Appeals { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
}