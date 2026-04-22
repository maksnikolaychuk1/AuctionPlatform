using System.Net;
using System.Net.Http.Json;
using AuctionPlatform.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace AuctionPlatform.Tests.Integration;

public class AuctionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuctionApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/auctions");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetAuctionById_NonExistent_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/auctions/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostAuction_ValidData_ReturnsCreated()
    {
        // Arrange
        var newAuction = new Auction { Title = "Test", StartingPrice = 10 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auctions", newAuction);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostBid_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var auctionId = Guid.NewGuid();
        var bidData = new { BidderId = Guid.NewGuid(), Amount = 100 };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/auctions/{auctionId}/bids", bidData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PatchCloseAuction_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var auctionId = Guid.NewGuid();

        // Act
        var response = await _client.PatchAsync($"/api/auctions/{auctionId}/close", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAuctionBids_ReturnsSuccess()
    {
        // Arrange
        var auctionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/auctions/{auctionId}/bids");

        // Assert
        response.EnsureSuccessStatusCode();
    }
    
    [Fact] 
    public async Task CreateAndGetAuction_FlowWorks()
    { 
        // Arrange
        var newAuction = new Auction { Title = "Flow" };
        var createResponse = await _client.PostAsJsonAsync("/api/auctions", newAuction);
        var createdAuction = await createResponse.Content.ReadFromJsonAsync<Auction>();

        // Act
        var getResponse = await _client.GetAsync($"/api/auctions/{createdAuction!.Id}");

        // Assert
        getResponse.EnsureSuccessStatusCode();
    }

    [Fact] 
    public async Task PostBid_AuctionActive_ReturnsBadRequestDueToPriceLimit()
    { 
        // Arrange
        var newAuction = new Auction 
        { 
            Title = "Test", 
            Status = AuctionStatus.Active, 
            CurrentPrice = 500, 
            EndTime = DateTime.UtcNow.AddDays(1) 
        };
        var createResponse = await _client.PostAsJsonAsync("/api/auctions", newAuction);
        var auction = await createResponse.Content.ReadFromJsonAsync<Auction>();
        
        var bidData = new { BidderId = Guid.NewGuid(), Amount = 10 };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/auctions/{auction!.Id}/bids", bidData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest); 
    }
}