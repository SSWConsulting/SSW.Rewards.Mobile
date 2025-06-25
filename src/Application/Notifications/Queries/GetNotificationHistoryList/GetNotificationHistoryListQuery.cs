using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Notifications;

namespace SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;

public class GetNotificationHistoryListQuery : IRequest<NotificationHistoryListViewModel>, IPagedRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
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

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(n => n.Title != null && n.Title.Contains(request.Search));
            }

            var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            query = request.SortLabel switch
            {
                "Title" => isDesc ? query.OrderByDescending(n => n.Title) : query.OrderBy(n => n.Title),
                "CreatedDate" => isDesc ? query.OrderByDescending(n => n.CreatedUtc) : query.OrderBy(n => n.CreatedUtc),
                "WasSent" => isDesc ? query.OrderByDescending(n => n.WasSent) : query.OrderBy(n => n.WasSent),
                "HasError" => isDesc ? query.OrderByDescending(n => n.HasError) : query.OrderBy(n => n.HasError),
                "SentOn" => isDesc ? query.OrderByDescending(n => n.SentOn) : query.OrderBy(n => n.SentOn),
                "NumberOfUsersTargeted" => isDesc ? query.OrderByDescending(n => n.NumberOfUsersTargeted) : query.OrderBy(n => n.NumberOfUsersTargeted),
                "NumberOfUsersSent" => isDesc ? query.OrderByDescending(n => n.NumberOfUsersSent) : query.OrderBy(n => n.NumberOfUsersSent),
                _ => query.OrderByDescending(n => n.CreatedUtc),
            };

            var projected = query
                .ProjectTo<NotificationHistoryDto>(_mapper.ConfigurationProvider);

            return await projected.ToPaginatedResultAsync<NotificationHistoryListViewModel, NotificationHistoryDto>(request, cancellationToken);
        }
    }
}