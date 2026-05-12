using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using rukpokwufestivalapi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace rukpokwufestivalapi.test.Shared
{
    public class CustomIntegrationWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(static services =>
            {
                // 1. Find the real DbContext registration and remove it
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<FestivalDbContext>));

                if (descriptor != null) services.Remove(descriptor);

                // 2. Add a fresh SQLite In-Memory database
                services.AddDbContext<FestivalDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestFestivalDb");
                });

                // 3. (Optional) Seed some fake pageant data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<FestivalDbContext>();
                db.Database.EnsureCreated();

                //if (!db.Contestants.Any())
                //{
                //    db.Contestants.Add(new Contestant { Name = "Test Queen", Bio = "Sample bio" });
                //    db.SaveChanges();
                //}
            });
        }
    }

    public class PageantTests : IClassFixture<CustomIntegrationWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PageantTests(CustomIntegrationWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetContestants_ShouldReturnSeededData()
        {
            // Act
            var response = await _client.GetAsync("/api/contestants");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Test Queen", content);
        }
    }
}
