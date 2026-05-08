using Microsoft.EntityFrameworkCore;

namespace rukpokwufestivalapi.Infrastructure
{
    public class FestivalDbContext : DbContext
    {
        public FestivalDbContext(DbContextOptions<FestivalDbContext> options) : base(options)
        {
                        
        }
    }
}
