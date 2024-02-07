using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Common.Extensions;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.GetNewUsers;

public class GetNewUsersQuery : IRequest<NewUsersViewModel>
{
    public LeaderboardFilter Filter { get; set; }
    public bool FilterStaff { get; set; }
}

public class GetNewUsersHandler(
    IApplicationDbContext context,
    IMapper mapper,
    IDateTime dateTime)
    : IRequestHandler<GetNewUsersQuery, NewUsersViewModel>
{
    public async Task<NewUsersViewModel> Handle(GetNewUsersQuery request, CancellationToken cancellationToken)
    {
        var eligibleUsers = context.Users.Where(u => u.Activated == true);

        if (request.Filter == LeaderboardFilter.ThisYear)
        {
            eligibleUsers = eligibleUsers
                .TagWith("NewUsersThisYear")
                .Where(u => u.CreatedUtc.Year == dateTime.Now.Year);
        }
        else if (request.Filter == LeaderboardFilter.ThisMonth)
        {
            eligibleUsers = eligibleUsers
                .TagWith("NewUsersThisMonth")
                .Where(u => u.CreatedUtc.Year == dateTime.Now.Year && u.CreatedUtc.Month == dateTime.Now.Month);
        }
        else if (request.Filter == LeaderboardFilter.Today)
        {
            eligibleUsers = eligibleUsers
                .TagWith("NewUsersToday")
                .Where(u => u.CreatedUtc.Year == dateTime.Now.Year && u.CreatedUtc.Month == dateTime.Now.Month && u.CreatedUtc.Day == dateTime.Now.Day);
        }
        else if (request.Filter == LeaderboardFilter.ThisWeek)
        {
            var start = dateTime.Now.FirstDayOfWeek();
            var end = start.AddDays(7);
            
            eligibleUsers = eligibleUsers
                .TagWith("NewUsersThisWeek")
                .Where(u => start <= u.CreatedUtc && u.CreatedUtc <= end);
        }
        else if (request.Filter == LeaderboardFilter.Forever)
        {
            // no action
        }

        eligibleUsers = eligibleUsers
            .Include(u => u.UserAchievements);

        var users = await eligibleUsers
            .ProjectTo<NewUserDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // filter out any @ssw.com.au users (.ends with didn't translate with EF Core :(
        if (request.FilterStaff)
        {
            users = users.Where(u => (u.Email != null && !u.Email.EndsWith("@ssw.com.au", StringComparison.OrdinalIgnoreCase))).ToList();
        }

        NewUsersViewModel vm = new()
        {
            NewUsers = users
        };

        return vm;
    }
}