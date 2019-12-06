using AutoMapper;
using SSW.Rewards.Domain.Entities;
using System.Linq;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<StaffMember, StaffDto>()
                .ForMember(dst => dst.Skills, opt => opt.MapFrom(s => s.StaffMemberSkills.Select(sms => sms.Skill.Name)));
        }
    }
}
