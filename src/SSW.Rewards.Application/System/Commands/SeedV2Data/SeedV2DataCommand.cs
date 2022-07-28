using SSW.Rewards.Application.System.Commands.Common;

namespace SSW.Rewards.Application.System.Commands.SeedV2Data;

public class SeedV2DataCommand : IRequest
{
}

public class SeedV2DataCommandHandler : IRequestHandler<SeedV2DataCommand>
{
    private readonly IApplicationDbContext _context;

    public SeedV2DataCommandHandler(IApplicationDbContext context)
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
