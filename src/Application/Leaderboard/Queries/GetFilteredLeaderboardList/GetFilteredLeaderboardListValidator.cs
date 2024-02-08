namespace SSW.Rewards.Application.Leaderboard.Queries.GetFilteredLeaderboardList;
public class GetFilteredLeaderboardListValidator : AbstractValidator<GetFilteredLeaderboardListQuery>
{
    public GetFilteredLeaderboardListValidator()
    {
        RuleFor(q => q.Filter)
            .Must(BeValidFilter)
            .WithMessage("Invalid filter");
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
