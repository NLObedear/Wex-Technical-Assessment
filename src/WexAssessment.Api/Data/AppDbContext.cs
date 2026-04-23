using Microsoft.EntityFrameworkCore;
using WexAssessment.Api.Models;

namespace WexAssessment.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CreditLimit).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasPrecision(18, 2);
            e.HasOne(t => t.Card)
             .WithMany(c => c.Transactions)
             .HasForeignKey(t => t.CardId);
        });
    }
}