using FluentValidation;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetFilteredLeaderboardList
{
    public class GetFilteredListQueryValidator : AbstractValidator<GetFilteredLeaderboardListQuery>
    {
        public GetFilteredListQueryValidator()
        {
            RuleFor(q => q.Filter)
                .Must(BeValidFilter);
        }

        public bool BeValidFilter(LeaderboardFilter filter)
        {
            if (filter == LeaderboardFilter.ThisMonth || filter == LeaderboardFilter.ThisYear)
                return true;

            return false;
        }
    }
}
