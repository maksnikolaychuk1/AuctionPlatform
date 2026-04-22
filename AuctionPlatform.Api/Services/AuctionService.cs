using AuctionPlatform.Api.Models;
using AuctionPlatform.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Api.Services;

public interface IAuctionService {
    Task<Bid> PlaceBidAsync(Guid auctionId, Guid bidderId, decimal amount);
    Task CloseAuctionAsync(Guid auctionId);
}

public class AuctionService : IAuctionService {
    private readonly ApplicationDbContext _context;

    public AuctionService(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<Bid> PlaceBidAsync(Guid auctionId, Guid bidderId, decimal amount) {
        var auction = await _context.Auctions.FindAsync(auctionId);
        if (auction == null) throw new KeyNotFoundException("Auction not found");

        if (auction.Status != AuctionStatus.Active)
            throw new InvalidOperationException("Cannot bid on inactive auction");
        if (auction.SellerId == bidderId)
            throw new InvalidOperationException("Seller cannot bid on their own auction");
        if (DateTime.UtcNow > auction.EndTime)
            throw new InvalidOperationException("Cannot bid after auction has ended");
        if (amount <= auction.CurrentPrice || amount <= auction.StartingPrice)
            throw new InvalidOperationException("Bid must be higher than current price");

        var bid = new Bid { Id = Guid.NewGuid(), AuctionId = auctionId, BidderId = bidderId, Amount = amount, PlacedAt = DateTime.UtcNow };
        auction.CurrentPrice = amount;
        
        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();
        return bid;
    }

    public async Task CloseAuctionAsync(Guid auctionId) {
        var auction = await _context.Auctions.Include(a => a.Bids).FirstOrDefaultAsync(a => a.Id == auctionId);
        if (auction == null) throw new KeyNotFoundException("Auction not found");
        if (auction.Status == AuctionStatus.Ended) return;

        auction.Status = AuctionStatus.Ended;
        var highestBid = auction.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
        if (highestBid != null) auction.WinnerId = highestBid.BidderId;

        await _context.SaveChangesAsync();
    }
}