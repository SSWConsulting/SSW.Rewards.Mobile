using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList
{
    public class GetNotificationHistoryListQuery : IRequest<NotificationHistoryListViewModel>
    {
        public sealed class GetNotificationHistoryListQueryHandler : IRequestHandler<GetNotificationHistoryListQuery,NotificationHistoryListViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public GetNotificationHistoryListQueryHandler(IMapper mapper, ISSWRewardsDbContext context)
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
}