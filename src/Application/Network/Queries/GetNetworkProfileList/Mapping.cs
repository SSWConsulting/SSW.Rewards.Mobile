using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Network.Queries;

public class NetworkMap
{
    public StaffMember Staff { get; set; }
    public User User { get; set; }
}

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<NetworkMap, NetworkProfileDto>()
            .ForMember(d => d.UserId, opt => opt.MapFrom(u => u.User.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Staff.Name))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Staff.Email))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Staff.Title))
            .ForMember(d => d.ProfilePicture, opt => opt.MapFrom(s => s.User.Avatar))
            .ForMember(d => d.TotalPoints,
                opt => opt.MapFrom(s => s.User.UserAchievements.Sum(ua => ua.Achievement.Value)))
            .ForMember(d => d.Rank, opt => opt.Ignore())
            .ForMember(d => d.Scanned, opt => opt.Ignore());
    }
}