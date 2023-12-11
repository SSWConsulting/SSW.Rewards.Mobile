using AutoMapper.QueryableExtensions;

namespace SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;

public class GetNotificationHistoryListQuery : IRequest<NotificationHistoryListViewModel>
{
    public sealed class GetNotificationHistoryListQueryHandler : IRequestHandler<GetNotificationHistoryListQuery,NotificationHistoryListViewModel>
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
            var result = await _context
                .Notifications
                .ProjectTo<NotificationHistoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new NotificationHistoryListViewModel()
            {
                List = result
            };
        }
    }
}