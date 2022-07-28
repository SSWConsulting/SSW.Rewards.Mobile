using AutoMapper;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Achievement, AchievementAdminViewModel>()
                .ForMember(x => x.IsArchived, expression => expression.MapFrom(x => x.IsDeleted));
        }
    }
}
