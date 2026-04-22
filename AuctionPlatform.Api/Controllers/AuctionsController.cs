using AuctionPlatform.Api.Models;
using AuctionPlatform.Api.Repositories;
using AuctionPlatform.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase {
    private readonly ApplicationDbContext _context;
    private readonly IAuctionService _auctionService;

    public AuctionsController(ApplicationDbContext context, IAuctionService auctionService) {
        _context = context;
        _auctionService = auctionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuctions([FromQuery] AuctionStatus? status) {
        var query = _context.Auctions.AsQueryable();
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuction(Guid id) {
        var auction = await _context.Auctions.Include(a => a.Bids).FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null) return NotFound();
        return Ok(auction);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuction([FromBody] Auction auction) {
        auction.Id = Guid.NewGuid();
        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAuction), new { id = auction.Id }, auction);
    }

    [HttpPost("{id}/bids")]
    public async Task<IActionResult> PlaceBid(Guid id, [FromBody] BidRequest request) {
        try {
            var bid = await _auctionService.PlaceBidAsync(id, request.BidderId, request.Amount);
            return Ok(bid);
        } catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException) {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/bids")]
    public async Task<IActionResult> GetBids(Guid id) {
        var bids = await _context.Bids.Where(b => b.AuctionId == id).ToListAsync();
        return Ok(bids);
    }

    [HttpPatch("{id}/close")]
    public async Task<IActionResult> CloseAuction(Guid id) {
        try {
            await _auctionService.CloseAuctionAsync(id);
            return Ok();
        } catch (KeyNotFoundException ex) {
            return NotFound(new { error = ex.Message });
        }
    }
}

public class BidRequest {
    public Guid BidderId { get; set; }
    public decimal Amount { get; set; }
}