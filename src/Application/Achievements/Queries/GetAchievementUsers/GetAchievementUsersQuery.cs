using Shared.DTOs.Achievements;
using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementUsers;

public class GetAchievementUsersQuery : IRequest<AchievementUsersViewModel>
{
    public int AchievementId { get; set; }
}

public class GetAchievementUsersQueryHandler : IRequestHandler<GetAchievementUsersQuery, AchievementUsersViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAchievementUsersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AchievementUsersViewModel> Handle(GetAchievementUsersQuery request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements
            .Include(a => a.UserAchievements)
            .ThenInclude(ua => ua.User)
            .FirstOrDefaultAsync(a => a.Id == request.AchievementId, cancellationToken);

        if (achievement == null)
        {
            throw new NotFoundException(nameof(Achievement), request.AchievementId);
        }

        var achievementUsers = achievement.UserAchievements
            .OrderByDescending(ua => ua.AwardedAt)
            .Select(ua => _mapper.Map<AchievementUserDto>(ua));

        return new AchievementUsersViewModel
        {
            AchievementName = achievement.Name,
            Users = achievementUsers
        };
    }
}