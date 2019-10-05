using MediatR;
using SSW.Consulting.Application.Common.Interfaces;
using SSW.Consulting.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.System.Commands.SeedData
{
    public class SeedSampleDataCommand : IRequest
    {
    }

    public class SeedSampleDataCommandHandler : IRequestHandler<SeedSampleDataCommand>
    {
        private readonly ISSWConsultingDbContext _context;

        public SeedSampleDataCommandHandler(ISSWConsultingDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(SeedSampleDataCommand request, CancellationToken cancellationToken)
        {
            var seeder = new SampleDataSeeder(_context);

            await seeder.SeedAllAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
