using AuctionPlatform.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Api.Repositories;

public class ApplicationDbContext : DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // Конфігурація зв'язків
        modelBuilder.Entity<Bid>()
            .HasOne<Auction>()
            .WithMany(a => a.Bids)
            .HasForeignKey(b => b.AuctionId);
    }
}