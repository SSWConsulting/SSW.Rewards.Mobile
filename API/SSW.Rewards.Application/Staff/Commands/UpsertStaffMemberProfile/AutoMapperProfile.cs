using AutoMapper;
using SSW.Rewards.Application.Staff.Queries.GetStaffList;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Staff.Commands.UpsertStaffMemberProfile
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UpsertStaffMemberProfileCommand, StaffMember>();
            CreateMap<UpsertStaffMemberProfileCommand, StaffDto>();
        }
    }
}
