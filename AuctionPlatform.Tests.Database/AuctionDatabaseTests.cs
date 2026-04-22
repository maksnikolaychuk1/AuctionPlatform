using AuctionPlatform.Api.Models;
using AuctionPlatform.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;

namespace AuctionPlatform.Tests.Database;

public class AuctionDatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
        
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;
            
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task SaveAuction_MaintainsRelations()
    {
        // Arrange
        var a = new Auction { Id = Guid.NewGuid() };
        var b = new Bid { Id = Guid.NewGuid(), AuctionId = a.Id, Amount = 100 };
        
        _context.Auctions.Add(a);
        _context.Bids.Add(b);
        await _context.SaveChangesAsync();

        // Act
        var saved = await _context.Auctions.Include(x => x.Bids).FirstAsync();

        // Assert
        saved.Bids.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AddUser_SavesToDatabase()
    {
        // Arrange
        var u = new User { Id = Guid.NewGuid(), Username = "Test" };
        _context.Users.Add(u);
        await _context.SaveChangesAsync();

        // Act
        var saved = await _context.Users.FindAsync(u.Id);

        // Assert
        saved.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateAuctionStatus_Persists()
    {
        // Arrange
        var a = new Auction { Id = Guid.NewGuid(), Status = AuctionStatus.Upcoming };
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act
        a.Status = AuctionStatus.Active;
        await _context.SaveChangesAsync();
        
        var saved = await _context.Auctions.FindAsync(a.Id);

        // Assert
        saved!.Status.ShouldBe(AuctionStatus.Active);
    }

    [Fact]
    public async Task DeleteAuction_Removes()
    {
        // Arrange
        var a = new Auction { Id = Guid.NewGuid() };
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act
        _context.Auctions.Remove(a);
        await _context.SaveChangesAsync();
        
        var saved = await _context.Auctions.FindAsync(a.Id);

        // Assert
        saved.ShouldBeNull();
    }

    [Fact]
    public async Task FilterActiveAuctions_Works()
    {
        // Arrange
        _context.Auctions.Add(new Auction { Id = Guid.NewGuid(), Status = AuctionStatus.Active });
        _context.Auctions.Add(new Auction { Id = Guid.NewGuid(), Status = AuctionStatus.Ended });
        await _context.SaveChangesAsync();

        // Act
        var active = await _context.Auctions.Where(a => a.Status == AuctionStatus.Active).ToListAsync();

        // Assert
        active.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AddBid_UpdatesCount()
    {
        // Arrange
        var a = new Auction { Id = Guid.NewGuid() };
        _context.Auctions.Add(a);
        _context.Bids.Add(new Bid { Id = Guid.NewGuid(), AuctionId = a.Id });
        await _context.SaveChangesAsync();

        // Act
        var count = await _context.Bids.CountAsync();

        // Assert
        count.ShouldBe(1);
    }
}