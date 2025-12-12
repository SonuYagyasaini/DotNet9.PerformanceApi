using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(b =>
        {
            b.ToTable("Invoices");
            b.HasKey(x => x.Id);
            b.Property(x => x.Customer).HasMaxLength(200).IsRequired();
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");
        });
        base.OnModelCreating(modelBuilder);
    }
}
