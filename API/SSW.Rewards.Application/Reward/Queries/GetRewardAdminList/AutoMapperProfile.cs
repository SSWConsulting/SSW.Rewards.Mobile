using AutoMapper;

namespace SSW.Rewards.Application.Reward.Queries.GetRewardAdminList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Reward, RewardAdminViewModel>();
        }
    }
}
