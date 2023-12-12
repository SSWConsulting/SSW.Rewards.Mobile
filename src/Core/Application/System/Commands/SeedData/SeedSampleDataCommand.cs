using SSW.Rewards.Application.System.Commands.Common;

namespace SSW.Rewards.Application.System.Commands.SeedData;

public class SeedSampleDataCommand : IRequest
{
}

public class SeedSampleDataCommandHandler : IRequestHandler<SeedSampleDataCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IProfileStorageProvider _storageProvider;

    public SeedSampleDataCommandHandler(
        IApplicationDbContext context,
        IProfileStorageProvider storageProvider)
    {
        _context = context;
        _storageProvider = storageProvider;
    }

    public async Task Handle(SeedSampleDataCommand request, CancellationToken cancellationToken)
    {
        var seeder = new SampleDataSeeder(_context);
        var profileData = await _storageProvider.GetBlob("NDC-Profiles-2019.xlsx");
        await seeder.SeedAllAsync(profileData, cancellationToken);
    }
}
