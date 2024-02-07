using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Common.Extensions;

namespace SSW.Rewards.Application.Users.Queries.GetNewUsers;
    
// TODO: something went wrong with this query. It should just be returning users based on the filter, nothing to do with staff or achievements
public class NewUserDto
{
    public int? UserId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }
}

public class NewUsersViewModel
{
    public IEnumerable<NewUserDto> NewUsers;
}

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
        // find all the activated users with enough points in the (today/month/year/forever) leaderboard to claim the specific reward 
        var eligibleUsers = context.Users.Where(u => u.Activated == true);

        if (request.Filter == LeaderboardFilter.ThisYear)
        {
            eligibleUsers = eligibleUsers
                .TagWith("PointsThisYear")
                .Where(u => u.CreatedUtc.Year == dateTime.Now.Year);
        }
        else if (request.Filter == LeaderboardFilter.ThisMonth)
        {
            eligibleUsers = eligibleUsers
                .TagWith("PointsThisMonth")
                .Where(u => u.CreatedUtc.Year == dateTime.Now.Year && u.CreatedUtc.Month == dateTime.Now.Month);
        }
        else if (request.Filter == LeaderboardFilter.Today)
        {
            eligibleUsers = eligibleUsers
                .TagWith("PointsToday")
                .Where(u => u.CreatedUtc.Year == dateTime.Now.Year && u.CreatedUtc.Month == dateTime.Now.Month && u.CreatedUtc.Day == dateTime.Now.Day);
        }
        else if (request.Filter == LeaderboardFilter.ThisWeek)
        {
            var start = dateTime.Now.FirstDayOfWeek();
            var end = start.AddDays(7);
            // TODO: Find a better way - EF Can't translate our extension method -- so writing the date range comparison directly in linq for now
            eligibleUsers = eligibleUsers
                .TagWith("PointsThisWeek")
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