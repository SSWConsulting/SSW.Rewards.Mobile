using AutoMapper;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<StaffMember, StaffDto>()
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.StaffMemberSkills))
                .ForMember(dest => dest.Scanned, opt => opt.Ignore());

            CreateMap<StaffMemberSkill, StaffSkillDto>()
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Skill.Name))
                .ForMember(dst => dst.Level, opt => opt.MapFrom(src => src.Level));
        }
    }
}
