using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Common;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<Reward, RewardDto>();
    }
}
