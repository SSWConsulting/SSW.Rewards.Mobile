using AutoMapper;

namespace SSW.Rewards.Application.Reward.Queries.GetRewardList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Reward, RewardViewModel>();
        }
    }
}
