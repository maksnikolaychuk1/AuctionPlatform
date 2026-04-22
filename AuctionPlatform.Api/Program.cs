using AuctionPlatform.Api.Models;
using AuctionPlatform.Api.Repositories;
using AuctionPlatform.Api.Services;
using Bogus;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Отримуємо рядок підключення з конфігурації
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Налаштовуємо DB Context на PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); 

builder.Services.AddScoped<IAuctionService, AuctionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Сідінг даних (Bogus) - 10 000 записів
using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    if (!context.Auctions.Any()) {
        Console.WriteLine("Generating 10,000 records...");
        
        var users = new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.Username, f => f.Internet.UserName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Balance, f => f.Finance.Amount(500, 10000))
            .Generate(2000);
        
        var auctions = new Faker<Auction>()
            .RuleFor(a => a.Id, f => Guid.NewGuid())
            .RuleFor(a => a.Title, f => f.Commerce.ProductName())
            .RuleFor(a => a.Description, f => f.Commerce.ProductDescription())
            .RuleFor(a => a.StartingPrice, f => f.Finance.Amount(10, 500))
            .RuleFor(a => a.CurrentPrice, (f, a) => a.StartingPrice)
            .RuleFor(a => a.StartTime, f => f.Date.Past(1))
            .RuleFor(a => a.EndTime, f => f.Date.Future(1))
            .RuleFor(a => a.Status, f => f.PickRandom<AuctionStatus>())
            .RuleFor(a => a.SellerId, f => f.PickRandom(users).Id)
            .Generate(3000);

        var bids = new Faker<Bid>()
            .RuleFor(b => b.Id, f => Guid.NewGuid())
            .RuleFor(b => b.AuctionId, f => f.PickRandom(auctions).Id)
            .RuleFor(b => b.BidderId, f => f.PickRandom(users).Id)
            .RuleFor(b => b.Amount, f => f.Finance.Amount(501, 2000))
            .RuleFor(b => b.PlacedAt, f => f.Date.Recent())
            .Generate(5000);

        context.Users.AddRange(users);
        context.Auctions.AddRange(auctions);
        context.Bids.AddRange(bids);
        context.SaveChanges();
        Console.WriteLine("Seeding completed.");
    }
}

app.Run();

// Робимо Program публічним для доступу з інтеграційних тестів
public partial class Program { }