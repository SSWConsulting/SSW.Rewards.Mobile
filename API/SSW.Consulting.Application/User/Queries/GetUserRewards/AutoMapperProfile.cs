using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Application.User.Queries.GetUserRewards
{
    class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<JoinedUserReward, UserRewardViewModel>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src.UserReward != null ? src.UserReward.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Awarded, opt => opt.MapFrom(src => src.UserReward != null));
        }
    }
}
