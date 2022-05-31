using AutoMapper;
using System.Linq;

namespace SSW.Rewards.Application.Users.Queries.GetCurrentUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
			CreateMap<Domain.Entities.User, CurrentUserViewModel>()
				.ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.Balance, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value) - src.UserRewards.Sum(ur => ur.Reward.Cost)))
				.ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar));
		}
    }
}
