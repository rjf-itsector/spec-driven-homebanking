using HomeBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<HomeBankingDbContext>(options =>
    options.UseInMemoryDatabase("HomeBanking"));

var app = builder.Build();

// Seed demo data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HomeBankingDbContext>();
    await DataSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.Run();
