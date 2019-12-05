using AutoMapper;
using SSW.Rewards.Application.User.Commands.UpsertUser;

namespace HW.KNOWnoise.Application.Admin.Users.Commands.UpsertUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UpsertUserCommand, SSW.Rewards.Domain.Entities.User>();
        }
    }
}
