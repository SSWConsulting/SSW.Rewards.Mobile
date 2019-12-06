using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.System.Commands.SeedData
{
    public class SeedSampleDataCommand : IRequest
    {
    }

    public class SeedSampleDataCommandHandler : IRequestHandler<SeedSampleDataCommand>
    {
        private readonly ISSWRewardsDbContext _context;
        private readonly IProfileStorageProvider _storageProvider;

        public SeedSampleDataCommandHandler(
            ISSWRewardsDbContext context,
            IProfileStorageProvider storageProvider)
        {
            _context = context;
            _storageProvider = storageProvider;
        }

        public async Task<Unit> Handle(SeedSampleDataCommand request, CancellationToken cancellationToken)
        {
            var seeder = new SampleDataSeeder(_context);
            var profileData = await _storageProvider.GetProfileData();
            await seeder.SeedAllAsync(profileData, cancellationToken);

            return Unit.Value;
        }
    }
}
