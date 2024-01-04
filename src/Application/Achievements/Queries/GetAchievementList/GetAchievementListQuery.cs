using AutoMapper.QueryableExtensions;
using Shared.DTOs.Achievements;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementList;

public class GetAchievementListQuery : IRequest<AchievementListViewModel> { }

public sealed class GetAchievementListQueryHandler : IRequestHandler<GetAchievementListQuery, AchievementListViewModel>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public GetAchievementListQueryHandler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<AchievementListViewModel> Handle(GetAchievementListQuery request, CancellationToken cancellationToken)
    {
        var achievements = await _context
            .Achievements
            .Where(a => !a.IsDeleted)
            .ProjectTo<AchievementDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new AchievementListViewModel
        {
            Achievements = achievements
        };
    }
}
