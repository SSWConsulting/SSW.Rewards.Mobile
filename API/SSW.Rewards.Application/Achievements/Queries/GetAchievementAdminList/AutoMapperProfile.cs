using AutoMapper;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Achievement, AchievementAdminViewModel>();
        }
    }
}
