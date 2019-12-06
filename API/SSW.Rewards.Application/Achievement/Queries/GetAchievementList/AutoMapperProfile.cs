using AutoMapper;

namespace SSW.Rewards.Application.Achievement.Queries.GetAchievementList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Achievement, AchievementViewModel>();
        }
    }
}
