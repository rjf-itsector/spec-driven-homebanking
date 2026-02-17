using Microsoft.EntityFrameworkCore;
using HomeBanking.Core.Entities;

namespace HomeBanking.Infrastructure.Data;

public class HomeBankingDbContext : DbContext
{
    public HomeBankingDbContext(DbContextOptions<HomeBankingDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Accounts)
                  .HasForeignKey(a => a.UserId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            entity.HasOne(t => t.Account)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(t => t.AccountId);
        });
    }
}
