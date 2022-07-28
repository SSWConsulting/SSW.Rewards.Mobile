using FluentValidation;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetFilteredLeaderboardList;
public class GetFilteredLeaderboardListValidator : AbstractValidator<GetFilteredLeaderboardList>
{
    public GetFilteredLeaderboardListValidator()
    {
        RuleFor(q => q.Filter)
            .Must(BeValidFilter)
            .WithMessage("Invalid filter");
    }

    public bool BeValidFilter(LeaderboardFilter filter)
    {
        if (filter == LeaderboardFilter.ThisMonth || filter == LeaderboardFilter.ThisYear)
            return true;

        return false;
    }
}
