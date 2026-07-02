using Microsoft.EntityFrameworkCore;
using Project.Base.Tests.Domain;

namespace Project.Base.Tests.Repository;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities { get; set; } = null!;
    public DbSet<NoStringEntity> NoStringEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired();
        });

        modelBuilder.Entity<NoStringEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.Value).IsRequired();
        });
    }
}
