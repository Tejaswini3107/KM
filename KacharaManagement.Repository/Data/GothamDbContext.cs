using KacharaManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace KacharaManagement.Repository.Data
{
    public class GothamDbContext : DbContext
    {
        public GothamDbContext(DbContextOptions<GothamDbContext> options) : base(options) { }

        public DbSet<SensorHistory> SensorHistories { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add any custom configuration here
        }
    }
}
