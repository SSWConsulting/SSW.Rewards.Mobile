using AutoMapper;

namespace SSW.Rewards.Application.Achievement.Queries.GetAchievementAdminList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Achievement, AchievementAdminViewModel>();
        }
    }
}
