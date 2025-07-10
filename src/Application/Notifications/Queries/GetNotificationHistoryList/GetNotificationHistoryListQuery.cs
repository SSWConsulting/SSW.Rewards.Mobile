using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Notifications;

namespace SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;

public class GetNotificationHistoryListQuery : IRequest<NotificationHistoryListViewModel>, IPagedRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
    public bool IncludeDeleted { get; set; }
    public string? SortLabel { get; set; }
    public string? SortDirection { get; set; }

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
                .TagWithContext("NotificationHistory");

            if (request.IncludeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(n => n.Title != null && n.Title.Contains(request.Search));
            }

            bool isDesc = request.SortDirection?.StartsWith("desc", StringComparison.OrdinalIgnoreCase) == true;

            var projected = query
                .ProjectTo<NotificationHistoryDto>(_mapper.ConfigurationProvider);

            // Status order: NotSent (0), Sent (1), Failed (2), Scheduled (3)
            projected = request.SortLabel switch
            {
                "Title" => isDesc
                    ? projected.OrderByDescending(n => n.Title)
                    : projected.OrderBy(n => n.Title),
                "CreatedDateUtc" => isDesc
                    ? projected.OrderByDescending(n => n.CreatedDateUtc)
                    : projected.OrderBy(n => n.CreatedDateUtc),
                "SentOn" => isDesc
                    ? projected.OrderByDescending(n => n.SentOn)
                    : projected.OrderBy(n => n.SentOn),
                "NumberOfUsersSent" => isDesc
                    ? projected.OrderByDescending(n => n.NumberOfUsersSent)
                    : projected.OrderBy(n => n.NumberOfUsersSent),
                "NumberOfUsersTargeted" => isDesc
                    ? projected.OrderByDescending(n => n.NumberOfUsersTargeted)
                    : projected.OrderBy(n => n.NumberOfUsersTargeted),
                "SentOrScheduled" => isDesc
                    ? projected.OrderByDescending(n => n.SentOn).ThenByDescending(n => n.ScheduledDate)
                    : projected.OrderBy(n => n.SentOn).ThenBy(n => n.ScheduledDate),
                "Delivered" => isDesc
                    ? projected.OrderByDescending(n => n.NumberOfUsersTargeted).ThenByDescending(n => n.NumberOfUsersSent)
                    : projected.OrderBy(n => n.NumberOfUsersTargeted).ThenBy(n => n.NumberOfUsersSent),
                "Status" => isDesc
                    ? projected.OrderByDescending(n => n.Status)
                    : projected.OrderBy(n => n.Status),
                _ => projected.OrderByDescending(n => n.CreatedDateUtc),
            };

            return await projected.ToPaginatedResultAsync<NotificationHistoryListViewModel, NotificationHistoryDto>(request, cancellationToken);
        }
    }
}