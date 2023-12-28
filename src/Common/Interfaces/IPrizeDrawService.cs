using Shared.DTOs.PrizeDraw;

namespace Shared.Interfaces;

public interface IPrizeDrawService
{
    Task<EligibleUsersViewModel> GetEligibleUsers(GetEligibleUsersFilter filter);
}
