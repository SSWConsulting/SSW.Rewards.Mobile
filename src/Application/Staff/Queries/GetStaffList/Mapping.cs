﻿using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffList;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<StaffMemberSkill, StaffSkillDto>()
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Skill.Name))
                .ForMember(dst => dst.Level, opt => opt.MapFrom(src => src.Level))
                .ForMember(dst => dst.ImageUri, opt => opt.MapFrom(src => src.Skill.ImageUri));

        CreateMap<StaffMember, StaffMemberDto>()
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.StaffMemberSkills))
                .ForMember(dest => dest.Points, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Scanned, opt => opt.Ignore());
    }
}
