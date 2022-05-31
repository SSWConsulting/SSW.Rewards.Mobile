using AutoMapper;
using SSW.Rewards.Application.Achievements.Queries.Common;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Achievement, AchievementDto>();
        }
    }
}
