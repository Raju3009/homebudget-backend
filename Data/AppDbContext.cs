using Microsoft.EntityFrameworkCore;
using HomeBudgetAPI.Models;

namespace HomeBudgetAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // 👇 Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        // 🔥 FIX decimal warning
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");
        }
    }
}