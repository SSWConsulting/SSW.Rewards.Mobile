using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Extensions;

namespace SSW.Rewards.Application.PrizeDraw.Queries;

public class GetEligibleUsers : IRequest<EligibleUsersViewModel>
{
    public int AchievementId { get; set; }
    public LeaderboardFilter Filter { get; set; }
    public bool FilterStaff { get; set; }
}

public class GetEligibleUsersHandler : IRequestHandler<GetEligibleUsers, EligibleUsersViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDateTime _dateTime;

    public GetEligibleUsersHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IDateTime dateTime)
    {
        _context = context;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<EligibleUsersViewModel> Handle(GetEligibleUsers request, CancellationToken cancellationToken)
    {
        // find all the activated users with enough points in the (today/month/year/forever) leaderboard to claim the specific reward 
        var eligibleUsers = _context.Users.Where(u => u.Activated == true);

        if (request.Filter == LeaderboardFilter.ThisYear)
        {
            eligibleUsers = eligibleUsers
                .TagWith("PointsThisYear")
                .Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year));
        }
        else if (request.Filter == LeaderboardFilter.ThisMonth)
        {
            eligibleUsers = eligibleUsers
                .TagWith("PointsThisMonth")
                .Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year && a.AwardedAt.Month == _dateTime.Now.Month));
        }
        else if (request.Filter == LeaderboardFilter.Today)
        {
            eligibleUsers = eligibleUsers
                .TagWith("PointsToday")
                .Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year && a.AwardedAt.Month == _dateTime.Now.Month && a.AwardedAt.Day == _dateTime.Now.Day));
        }
        else if (request.Filter == LeaderboardFilter.ThisWeek)
        {
            var start = _dateTime.Now.FirstDayOfWeek();
            var end = start.AddDays(7);
            // TODO: Find a better way - EF Can't translate our extension method -- so writing the date range comparison directly in linq for now
            eligibleUsers = eligibleUsers
                .TagWith("PointsThisWeek")
                .Where(u => u.UserAchievements.Any(a => start <= a.AwardedAt && a.AwardedAt <= end ));
        }
        else if (request.Filter == LeaderboardFilter.Forever)
        {
            // no action
        }

        eligibleUsers = eligibleUsers
            .Include(u => u.UserAchievements);

        string achievementName = string.Empty;
        if (request.AchievementId != 0)
        {
            var achievement = await _context.Achievements.FindAsync(request.AchievementId);
            if (achievement == null)
            {
                throw new NotFoundException(nameof(Achievement), request.AchievementId);
            }

            achievementName = achievement.Name;

            eligibleUsers = eligibleUsers
                .TagWith("PointsForAchievement")
                .Where(u => u.UserAchievements.Any(a => a.AchievementId == request.AchievementId));
        }

        var users = await eligibleUsers
            .ProjectTo<EligibleUserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // filter out any @ssw.com.au users (.ends with didn't translate with EF Core :(
        if (request.FilterStaff)
        {
            users = users.Where(u => (u.Email != null && !u.Email.EndsWith("@ssw.com.au", StringComparison.OrdinalIgnoreCase))).ToList();
        }

        EligibleUsersViewModel vm = new()
        {
            AchievementId = request.AchievementId,
            AchievementName = achievementName,
            EligibleUsers = users
        };

        vm.EligibleUsers = vm.EligibleUsers.OrderByDescending(u => u.Balance);

        return vm;
    }
}