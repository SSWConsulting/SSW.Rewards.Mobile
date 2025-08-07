namespace SSW.Rewards.Application.Notifications.Queries;

public class GetNumberOfImpactedNotificationUsersQuery : IRequest<int>
{
    public List<int> AchievementIds { get; set; } = [];

    public List<int> UserIds { get; set; } = [];

    public List<int> RoleIds { get; set; } = [];
}

public class GetNumberOfImpactedNotificationUsersQueryHandler : IRequestHandler<GetNumberOfImpactedNotificationUsersQuery, int>
{
    private readonly IApplicationDbContext _context;

    public GetNumberOfImpactedNotificationUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(GetNumberOfImpactedNotificationUsersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .GetTargetNotificationUserIdsQuery(request)
            .CountAsync(cancellationToken);
    }
}