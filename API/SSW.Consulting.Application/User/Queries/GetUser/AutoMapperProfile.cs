using AutoMapper;
using SSW.Consulting.Application.User.Queries.GetUser;
using SSW.Consulting.Domain.Entities;

namespace HW.KNOWnoise.Application.Admin.Users.Queries.GetUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserViewModel>();
        }
    }
}
