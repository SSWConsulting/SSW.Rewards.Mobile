using AutoMapper;
using SSW.Consulting.Domain.Entities;
using System.Linq;

namespace SSW.Consulting.Application.Staff.Queries.GetStaffList
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
