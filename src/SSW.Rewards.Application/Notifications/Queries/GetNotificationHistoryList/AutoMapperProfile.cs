using AutoMapper;

namespace SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<NotificationHistoryListViewModel, NotificationHistoryDto>();
            CreateMap<Domain.Entities.Notifications, NotificationHistoryDto>()
                .ForMember(dst => dst.EmailAddress, opt => opt.MapFrom(s => s.SentByStaffMember.Email ))
                .ForMember(dst => dst.CreatedDate, opt => opt.MapFrom(s => s.CreatedUtc));
        }
    }
}