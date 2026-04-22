using AuctionPlatform.Api.Models;
using AuctionPlatform.Api.Repositories;
using AuctionPlatform.Api.Services;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AuctionPlatform.Tests.Unit;

public class AuctionServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly ApplicationDbContext _context;
    private readonly AuctionService _sut;

    public AuctionServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new ApplicationDbContext(options);
        _sut = new AuctionService(_context);
    }

    private Auction CreateActiveAuction() => new Auction 
    { 
        Id = Guid.NewGuid(), 
        Status = AuctionStatus.Active, 
        EndTime = DateTime.UtcNow.AddDays(1), 
        StartingPrice = 50m, 
        CurrentPrice = 100m, 
        SellerId = Guid.NewGuid() 
    };

    [Fact]
    public async Task PlaceBid_ValidBid_UpdatesPrice()
    {
        // Arrange
        var a = CreateActiveAuction();
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act
        var res = await _sut.PlaceBidAsync(a.Id, Guid.NewGuid(), 150m);

        // Assert
        res.Amount.ShouldBe(150m);
    }

    [Fact]
    public async Task PlaceBid_AuctionNotFound_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Should.ThrowAsync<KeyNotFoundException>(() => 
            _sut.PlaceBidAsync(Guid.NewGuid(), Guid.NewGuid(), 150m));
    }

    [Fact]
    public async Task PlaceBid_InactiveAuction_ThrowsInvalidOperationException()
    {
        // Arrange
        var a = CreateActiveAuction();
        a.Status = AuctionStatus.Ended;
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => 
            _sut.PlaceBidAsync(a.Id, Guid.NewGuid(), 150m));
    }

    [Fact]
    public async Task PlaceBid_EndedAuctionByTime_ThrowsInvalidOperationException()
    {
        // Arrange
        var a = CreateActiveAuction();
        a.EndTime = DateTime.UtcNow.AddDays(-1);
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => 
            _sut.PlaceBidAsync(a.Id, Guid.NewGuid(), 150m));
    }

    [Fact]
    public async Task PlaceBid_BidLowerThanCurrent_ThrowsException()
    {
        // Arrange
        var a = CreateActiveAuction();
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => 
            _sut.PlaceBidAsync(a.Id, Guid.NewGuid(), 50m));
    }

    [Fact]
    public async Task PlaceBid_BidEqualToCurrent_ThrowsException()
    {
        // Arrange
        var a = CreateActiveAuction();
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => 
            _sut.PlaceBidAsync(a.Id, Guid.NewGuid(), 100m));
    }

    [Fact]
    public async Task PlaceBid_BidLowerThanStarting_ThrowsException()
    {
        // Arrange
        var a = CreateActiveAuction();
        a.CurrentPrice = 0;
        a.StartingPrice = 100;
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => 
            _sut.PlaceBidAsync(a.Id, Guid.NewGuid(), 50m));
    }

    [Fact]
    public async Task PlaceBid_SellerBidsOnOwn_ThrowsException()
    {
        // Arrange
        var a = CreateActiveAuction();
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => 
            _sut.PlaceBidAsync(a.Id, a.SellerId, 200m));
    }

    [Fact]
    public async Task CloseAuction_NotFound_ThrowsException()
    {
        // Act & Assert
        await Should.ThrowAsync<KeyNotFoundException>(() => 
            _sut.CloseAuctionAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CloseAuction_AlreadyEnded_DoesNothing()
    {
        // Arrange
        var a = CreateActiveAuction();
        a.Status = AuctionStatus.Ended;
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act
        await _sut.CloseAuctionAsync(a.Id);

        // Assert
        a.Status.ShouldBe(AuctionStatus.Ended);
    }

    [Fact]
    public async Task CloseAuction_NoBids_SetsEndedAndWinnerNull()
    {
        // Arrange
        var a = CreateActiveAuction();
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act
        await _sut.CloseAuctionAsync(a.Id);

        // Assert
        a.Status.ShouldBe(AuctionStatus.Ended);
        a.WinnerId.ShouldBeNull();
    }

    [Fact]
    public async Task CloseAuction_WithBids_SetsWinnerToHighest()
    {
        // Arrange
        var a = CreateActiveAuction();
        var winner = Guid.NewGuid();
        a.Bids.Add(new Bid { BidderId = Guid.NewGuid(), Amount = 150 });
        a.Bids.Add(new Bid { BidderId = winner, Amount = 200 });
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act
        await _sut.CloseAuctionAsync(a.Id);

        // Assert
        a.WinnerId.ShouldBe(winner);
    }

    [Fact]
    public async Task CloseAuction_WithBids_ChangesStatusToEnded()
    {
        // Arrange
        var a = CreateActiveAuction();
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act
        await _sut.CloseAuctionAsync(a.Id);

        // Assert
        a.Status.ShouldBe(AuctionStatus.Ended);
    }

    [Fact]
    public async Task PlaceBid_ValidBid_ReturnsBidObject()
    {
        // Arrange
        var a = CreateActiveAuction();
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act
        var bid = await _sut.PlaceBidAsync(a.Id, Guid.NewGuid(), 200m);

        // Assert
        bid.ShouldNotBeNull();
        bid.Amount.ShouldBe(200m);
    }

    [Fact]
    public async Task PlaceBid_AddsBidToContext()
    {
        // Arrange
        var a = CreateActiveAuction();
        _context.Auctions.Add(a);
        await _context.SaveChangesAsync();

        // Act
        await _sut.PlaceBidAsync(a.Id, Guid.NewGuid(), 300m);

        // Assert
        _context.Bids.Count().ShouldBe(1);
    }
}