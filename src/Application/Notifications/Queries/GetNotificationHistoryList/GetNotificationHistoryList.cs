﻿using SSW.Rewards.Shared.DTOs.Notifications;
using AutoMapper.QueryableExtensions;

namespace SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;
public class GetNotificationHistoryList : IRequest<List<NotificationHistoryDto>>
{
    
}

public class GetNotificationHistoryListQueryHandler : IRequestHandler<GetNotificationHistoryList, List<NotificationHistoryDto>>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public GetNotificationHistoryListQueryHandler(IMapper mapper, IApplicationDbContext context)
    {
        _mapper     = mapper;
        _context    = context;
    }

    public async Task<List<NotificationHistoryDto>> Handle(GetNotificationHistoryList request, CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .AsNoTracking()
            .TagWithContext("NotificationHistory")
            .ProjectTo<NotificationHistoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
