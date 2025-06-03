using Microsoft.EntityFrameworkCore;

namespace bingo_Tech.Models
{
    public class CallLogContext : DbContext
    {
        public CallLogContext(DbContextOptions<CallLogContext> options) : base(options) { }

        public DbSet<CallLog> CallLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CallLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CallerName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ReceiverName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
            });
        }
    }

}
