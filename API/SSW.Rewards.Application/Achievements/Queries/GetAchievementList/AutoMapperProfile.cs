using AutoMapper;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Achievement, AchievementViewModel>();
        }
    }
}
