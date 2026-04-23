
using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WexAssessment.Api.Data;
using WexAssessment.Api.Services;

namespace WexAssessment.Tests;
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// Configures the test web host by replacing the real database with an in-memory database
    /// and mocking the exchange rate service to avoid real HTTP calls to the Treasury API during testing
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d =>
                d.ServiceType.ToString().Contains("DbContext") ||
                d.ServiceType.ToString().Contains("DbContextOptions") ||
                d.ServiceType.ToString().Contains("Npgsql") ||
                d.ServiceType.ToString().Contains("EntityFramework")
            ).ToList();

            foreach (var s in toRemove)
                services.Remove(s);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            var mockExchange = new Mock<IExchangeRateService>();
            mockExchange
                .Setup(x => x.GetLatestRate(It.IsAny<string>()))
                .ReturnsAsync((1.5m, "2024-03-31"));
            mockExchange
                .Setup(x => x.GetRateForDate(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync((1.5m, "2024-03-31"));

            services.AddSingleton(mockExchange.Object);
        });

        builder.UseEnvironment("Testing");
    }
}
