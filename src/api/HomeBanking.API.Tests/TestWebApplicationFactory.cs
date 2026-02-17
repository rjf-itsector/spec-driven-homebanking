using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HomeBanking.Infrastructure.Data;

namespace HomeBanking.API.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<HomeBankingDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Also remove the DbContext itself to avoid duplicate registrations
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(HomeBankingDbContext));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Add test InMemory database with unique name
            services.AddDbContext<HomeBankingDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
}
