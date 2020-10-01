using AutoMapper;

namespace SSW.Rewards.Application.Reward.Queries.Common
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Reward, RewardViewModel>();
        }
    }
}
