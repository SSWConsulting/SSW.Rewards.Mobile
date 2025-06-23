using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Notifications;

namespace SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;

public class GetNotificationHistoryListQuery : IRequest<NotificationHistoryListViewModel>, IPagedRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }

    public sealed class GetNotificationHistoryListQueryHandler : IRequestHandler<GetNotificationHistoryListQuery, NotificationHistoryListViewModel>
    {
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;

        public GetNotificationHistoryListQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<NotificationHistoryListViewModel> Handle(GetNotificationHistoryListQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .TagWithContext("NotificationHistory")
                .OrderByDescending(n => n.CreatedUtc)
                .ProjectTo<NotificationHistoryDto>(_mapper.ConfigurationProvider);

            return await query.ToPaginatedResultAsync<NotificationHistoryListViewModel, NotificationHistoryDto>(request, cancellationToken);
        }
    }
}