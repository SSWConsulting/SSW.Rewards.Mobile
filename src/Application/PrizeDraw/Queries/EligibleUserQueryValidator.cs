namespace SSW.Rewards.Application.PrizeDraw.Queries;
public class EligibleUserQueryValidator : AbstractValidator<GetEligibleUsers>
{
    public EligibleUserQueryValidator()
    {
        //RuleFor(r => r.RewardId)
        //    .Must(r => r > 0);

        //RuleFor(r => r.Filter)
        //    .Must(BeValidFilter)
        //    .WithMessage("Invalid filter");
    }

    //public bool BeValidFilter(LeaderboardFilter filter)
    //{
    //    return 
    //        filter == LeaderboardFilter.Forever || 
    //        filter == LeaderboardFilter.Today ||
    //        filter == LeaderboardFilter.ThisMonth ||
    //        filter == LeaderboardFilter.ThisYear;
    //}
}