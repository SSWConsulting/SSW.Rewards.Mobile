using AutoMapper;
using System;
using System.Linq;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.User, LeaderboardUserDto>()
                .ForMember(dst => dst.UserId, opt => opt.Ignore())
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar == null ? null : new Uri(src.Avatar)))
                .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)));
        }
    }

}
