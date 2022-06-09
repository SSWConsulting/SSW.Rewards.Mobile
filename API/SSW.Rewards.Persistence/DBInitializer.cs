using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SSW.Rewards.Persistence
{
    public class DbInitializer
    {
        private readonly SSWRewardsDbContext _context;

        public DbInitializer(SSWRewardsDbContext context)
        {
            _context = context;
        }

        public async Task Run()
        {
            await _context.Database.MigrateAsync();
        }
    }
}
