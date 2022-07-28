using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.System.Commands.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.System.Commands.SeedV2Data
{
    public class SeedV2DataCommand : IRequest
    {
    }

    public class SeedV2DataCommandHandler : IRequestHandler<SeedV2DataCommand>
    {
        private readonly ISSWRewardsDbContext _context;

        public SeedV2DataCommandHandler(ISSWRewardsDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(SeedV2DataCommand request, CancellationToken cancellationToken)
        {
            var seeder = new SampleDataSeeder(_context);

            await seeder.SeedV2DataAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
