using AutoMapper;

namespace SSW.Consulting.Application.Reward.Queries.GetRewardList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Reward, RewardViewModel>();
        }
    }
}
