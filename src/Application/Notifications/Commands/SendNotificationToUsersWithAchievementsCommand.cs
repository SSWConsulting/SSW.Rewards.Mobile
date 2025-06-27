using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Services;

namespace SSW.Rewards.Application.Notifications.Commands;

public class SendNotificationToUsersWithAchievementsCommand : IRequest<NotificationSentResponse>
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? DataPayload { get; set; }
    public List<int> AchievementIds { get; set; } = [];
    public bool FilterStaff { get; set; }
    public string? ImageUrl { get; set; }
}

public class SendNotificationToUsersWithAchievementsCommandHandler : IRequestHandler<SendNotificationToUsersWithAchievementsCommand, NotificationSentResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IFirebaseNotificationService _firebaseNotificationService;
    private readonly ILogger<SendNotificationToUsersWithAchievementsCommandHandler> _logger;

    public SendNotificationToUsersWithAchievementsCommandHandler(
        IApplicationDbContext context,
        IFirebaseNotificationService firebaseNotificationService,
        ILogger<SendNotificationToUsersWithAchievementsCommandHandler> logger)
    {
        _context = context;
        _firebaseNotificationService = firebaseNotificationService;
        _logger = logger;
    }

    public async Task<NotificationSentResponse> Handle(SendNotificationToUsersWithAchievementsCommand request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .AsNoTracking()
            .TagWithContext()
            .Where(u => u.Activated && u.UserAchievements.Any(a => request.AchievementIds.Contains(a.AchievementId)));

        if (request.FilterStaff)
        {
            query = query.Where(u => u.Email != null && !u.Email.EndsWith("@ssw.com.au", StringComparison.OrdinalIgnoreCase));
        }

        var users = await query
            .Select(u => new
            {
                UserId = u.Id,
                Email = u.Email,
                HasDeviceTokens = u.DeviceTokens.Any()
            })
            .ToListAsync(cancellationToken);

        int notificationsSent = 0;
        foreach (var user in users)
        {
            if (!user.HasDeviceTokens)
            {
                _logger.LogWarning("User {UserId} ({Email}) does not have any device tokens. Notification not sent. AchievementIds: {AchievementIds}", user.UserId, user.Email, string.Join(", ", request.AchievementIds));
                continue;
            }

            var stats = await _firebaseNotificationService.SendNotificationAsync(
                user.UserId,
                request.Title,
                request.Message,
                request.ImageUrl,
                request.DataPayload ?? string.Empty,
                cancellationToken);

            notificationsSent += stats.Sent;
            if (stats.Sent == 0)
            {
                _logger.LogWarning("Failed to send notification to User {UserId} ({Email}). AchievementIds: {AchievementIds}", user.UserId, user.Email, string.Join(", ", request.AchievementIds));
            }
        }

        return new NotificationSentResponse
        {
            UsersToNotify = users.Count,
            NotificationsSent = notificationsSent
        };
    }
}
