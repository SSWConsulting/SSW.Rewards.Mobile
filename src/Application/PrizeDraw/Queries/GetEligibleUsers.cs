using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.PrizeDraw;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Extensions;

namespace SSW.Rewards.Application.PrizeDraw.Queries;

// TODO: something went wrong with this query. It should just be returning users based on the filter, nothing to do with staff or achievements
public class GetEligibleUsers : IRequest<EligibleUsersViewModel>
{
    public int? AchievementId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool FilterStaff { get; set; }
    public int Top { get; set; }
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
        var eligibleUsers = _context.Users
            .TagWithContext("GetUserByDateRange")
            .Where(u => u.Activated);

        if (request is { DateFrom: not null, DateTo: not null })
        {
            eligibleUsers = eligibleUsers
                .TagWith("PointsInDateRange")
                .Where(u => u.UserAchievements.Any(a => a.AwardedAt >= request.DateFrom && a.AwardedAt <= request.DateTo));
        }

        eligibleUsers = eligibleUsers
            .Include(u => u.UserAchievements);

        string achievementName = string.Empty;
        if (request.AchievementId is > 0)
        {
            var achievement = await _context.Achievements.FindAsync([request.AchievementId], cancellationToken);
            if (achievement == null)
            {
                throw new NotFoundException(nameof(Achievement), request.AchievementId);
            }

            achievementName = achievement.Name ?? string.Empty;

            eligibleUsers = eligibleUsers
                .TagWithContext("PointsForAchievement")
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
        
        if (request.Top > 0)
        {
            vm.EligibleUsers = vm.EligibleUsers.Take(request.Top);
        }

        return vm;
    }
}