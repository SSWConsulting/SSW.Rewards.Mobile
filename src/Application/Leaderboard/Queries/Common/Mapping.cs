using Shared.DTOs.Leaderboard;
using SSW.Rewards.Application.Common.Extensions;

namespace SSW.Rewards.Application.Leaderboard.Queries.Common;
public class Mapping : Profile
{
    public Mapping()
    {
        var start = DateTime.Now.FirstDayOfWeek();
        var end = DateTime.Now.FirstDayOfWeek().AddDays(7);

        CreateMap<User, LeaderboardUserDto>()
                .ForMember(dst => dst.Balance, opt => opt.Ignore())
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dst => dst.Rank, opt => opt.Ignore())
                .ForMember(dst => dst.TotalPoints, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.PointsClaimed, opt => opt.MapFrom(src => src.UserRewards.Sum(ur => ur.Reward.Cost)))

                // TODO:    Using DateTime.Now here presents instability for testing the queries dependent
                //          on this DTO. As discussed with williamliebenberg@ssw.com.au, we will accept
                //          this tech debt for now and investigate a better approach in the future. See
                //          https://github.com/SSWConsulting/SSW.Rewards.API/issues/7
                .ForMember(dst => dst.PointsThisYear, opt => opt.MapFrom(src => src.UserAchievements
                                                                                    .Where(ua => ua.AwardedAt.Year == DateTime.Now.Year)
                                                                                    .Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.PointsThisMonth, opt => opt.MapFrom(src => src.UserAchievements
                                                                                    .Where(ua => ua.AwardedAt.Year == DateTime.Now.Year && ua.AwardedAt.Month == DateTime.UtcNow.Month)
                                                                                    .Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.PointsToday, opt => opt.MapFrom(src => src.UserAchievements
                                                                                    .Where(ua => ua.AwardedAt.Year == DateTime.Now.Year && ua.AwardedAt.Month == DateTime.UtcNow.Month && ua.AwardedAt.Day == DateTime.UtcNow.Day)
                                                                                    .Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.PointsThisWeek, opt => opt.MapFrom(src => src.UserAchievements
                                                                                    .Where(ua => start <= ua.AwardedAt && ua.AwardedAt <= end)
                                                                                    .Sum(ua => ua.Achievement.Value)));
    }
}
