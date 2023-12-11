namespace SSW.Rewards.Application.Leaderboard.Queries.GetFilteredLeaderboardList;

public class GetFilteredListQueryValidator : AbstractValidator<GetFilteredLeaderboardListQuery>
{
    public GetFilteredListQueryValidator()
    {
        RuleFor(q => q.Filter)
            .Must(BeValidFilter);
    }

    public bool BeValidFilter(LeaderboardFilter filter)
    {
        return 
            filter == LeaderboardFilter.ThisMonth || 
            filter == LeaderboardFilter.ThisYear ||
            filter == LeaderboardFilter.ThisWeek ||
            filter == LeaderboardFilter.Today ||
            filter == LeaderboardFilter.Forever;
    }
}
