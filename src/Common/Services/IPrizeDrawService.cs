using SSW.Rewards.Shared.DTOs.PrizeDraw;

namespace SSW.Rewards.Shared.Services;

public interface IPrizeDrawService
{
    Task<EligibleUsersViewModel> GetEligibleUsers(GetEligibleUsersFilter filter, CancellationToken cancellationToken);
}
