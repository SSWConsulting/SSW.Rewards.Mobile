using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Network.Queries;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<(StaffMember, User), NetworkProfileDto>()
            .ForMember(d => d.UserId, opt => opt.MapFrom(u => u.Item2.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Item1.Name))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Item1.Email))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Item1.Title))
            .ForMember(d => d.ProfilePicture, opt => opt.MapFrom(s => s.Item2.Avatar))
            .ForMember(d => d.TotalPoints,
                opt => opt.MapFrom(s => s.Item2.UserAchievements.Sum(ua => ua.Achievement.Value)))
            .ForMember(d => d.Rank, opt => opt.Ignore())
            .ForMember(d => d.Scanned, opt => opt.Ignore());
    }
}