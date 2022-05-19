using AutoMapper;

namespace SSW.Rewards.Application.Rewards.Queries.Common
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Reward, RewardViewModel>();
        }
    }
}
