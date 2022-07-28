using AutoMapper;
using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;

public class NotificationHistoryDto : IMapFrom<Notification>
{
    public string Message { get; set; }
    public DateTime CreatedDate { get; set; }
    public string EmailAddress { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Notification, NotificationHistoryDto>()
            .ForMember(dst => dst.EmailAddress, opt => opt.MapFrom(s => s.SentByStaffMember.Email))
            .ForMember(dst => dst.CreatedDate, opt => opt.MapFrom(s => s.CreatedUtc));
    }
}