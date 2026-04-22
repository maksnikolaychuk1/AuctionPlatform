namespace AuctionPlatform.Api.Models;

public enum AuctionStatus { Upcoming, Active, Ended }

public class User {
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class Auction {
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AuctionStatus Status { get; set; }
    public Guid SellerId { get; set; }
    public Guid? WinnerId { get; set; }
    public List<Bid> Bids { get; set; } = new();
}

public class Bid {
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PlacedAt { get; set; }
}