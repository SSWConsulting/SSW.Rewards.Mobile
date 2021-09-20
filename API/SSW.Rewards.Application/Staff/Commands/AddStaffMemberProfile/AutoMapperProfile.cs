using AutoMapper;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Staff.Commands.AddStaffMemberProfile
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AddStaffMemberProfileCommand, StaffMember>();
        }
    }
}
