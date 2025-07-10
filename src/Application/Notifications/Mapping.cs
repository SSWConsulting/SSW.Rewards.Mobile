using SSW.Rewards.Shared.DTOs.Notifications;

namespace SSW.Rewards.Application.Notifications;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<Notification, NotificationHistoryDto>()
            .ForMember(dest => dest.CreatedDateUtc, opt => opt.MapFrom(src => src.CreatedUtc))
            .ForMember(dest => dest.ScheduledDate, opt => opt.MapFrom(src => src.Scheduled.HasValue ? src.Scheduled.Value.DateTime : default))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.SentByStaffMember.Email))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.NotificationTag))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.WasSent, opt => opt.MapFrom(src => src.WasSent))
            .ForMember(dest => dest.HasError, opt => opt.MapFrom(src => src.HasError))
            .ForMember(dest => dest.SentOn, opt => opt.MapFrom(src => src.SentOn.HasValue ? src.SentOn.Value.DateTime : (DateTime?)null))
            .ForMember(dest => dest.NumberOfUsersTargeted, opt => opt.MapFrom(src => src.NumberOfUsersTargeted))
            .ForMember(dest => dest.NumberOfUsersSent, opt => opt.MapFrom(src => src.NumberOfUsersSent))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.WasSent && !src.HasError ? NotificationStatus.Sent :
                src.HasError ? NotificationStatus.Failed :
                src.Scheduled != null && !src.WasSent && !src.HasError ? NotificationStatus.Scheduled :
                NotificationStatus.NotSent
            ))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
    }
}
