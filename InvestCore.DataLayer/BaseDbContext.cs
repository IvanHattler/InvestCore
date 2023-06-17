using InvestCore.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestCore.DataLayer
{
    public class BaseDbContext : DbContext
    {
        public DbSet<Portfolio> Portfolios { get; set; }

        public BaseDbContext(DbContextOptions<BaseDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Portfolio>()
                .HasKey(p => p.Id);
        }
    }
}
