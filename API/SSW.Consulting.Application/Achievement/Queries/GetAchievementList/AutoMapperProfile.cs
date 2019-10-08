using AutoMapper;

namespace SSW.Consulting.Application.Achievement.Queries.GetAchievementList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Achievement, AchievementViewModel>();
        }
    }
}
