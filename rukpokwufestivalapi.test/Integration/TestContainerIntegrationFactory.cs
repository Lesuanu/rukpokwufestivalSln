using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using rukpokwufestivalapi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using Testcontainers.PostgreSql;

namespace rukpokwufestivalapi.test.Integration
{
    public class TestContainerIntegrationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine") // You can specify a version
            .WithDatabase("FestivalDb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<FestivalDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add DbContext using the container's connection string
                services.AddDbContext<FestivalDbContext>(options =>
                    options.UseNpgsql(_dbContainer.GetConnectionString()));
            });
        }

        // Start the container before tests
        public async Task InitializeAsync() => await _dbContainer.StartAsync();

        // Stop the container after tests
        public new async Task DisposeAsync() => await _dbContainer.StopAsync();
    }

    public class PageantIntegrationTests : IClassFixture<TestContainerIntegrationFactory>
    {
        private readonly HttpClient _client;

        public PageantIntegrationTests(TestContainerIntegrationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task VoteForContestant_ShouldUpdateDatabase()
        {
            // 1. Arrange: Create a payload for a vote
            var voteData = new { ContestantId = 1, VoterId = "user_abc" };

            // 2. Act: POST to your Minimal API
            var response = await _client.PostAsJsonAsync("/api/vote", voteData);

            // 3. Assert
            response.EnsureSuccessStatusCode();
            // You can even query the DB directly here to verify the record exists!
        }
    }


}

