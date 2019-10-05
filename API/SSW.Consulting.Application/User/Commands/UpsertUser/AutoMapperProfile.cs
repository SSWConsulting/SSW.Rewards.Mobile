using AutoMapper;
using SSW.Consulting.Application.User.Commands.UpsertUser;

namespace HW.KNOWnoise.Application.Admin.Users.Commands.UpsertUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UpsertUserCommand, SSW.Consulting.Domain.Entities.User>();
        }
    }
}
