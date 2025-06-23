using System.ComponentModel.DataAnnotations;
using Hangfire;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Notifications.BackgroundJobs;
using SSW.Rewards.Application.Services;

namespace SSW.Rewards.Application.Notifications.Commands;

public class SendAdminNotificationCommand : IRequest<NotificationSentResponse>
{
    public DateTimeOffset? ScheduleAt { get; set; }

    public List<int> AchievementIds { get; set; } = [];

    public List<int> UserIds { get; set; } = [];

    public List<int> RoleIds { get; set; } = [];

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Body { get; set; } = string.Empty;

    [Url]
    public string? ImageUrl { get; set; }
}

public class SendAdminNotificationCommandHandler : IRequestHandler<SendAdminNotificationCommand, NotificationSentResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IFirebaseNotificationService _firebaseNotificationService;
    private readonly IDateTime _dateTimeService;
    private readonly ILogger<SendAdminNotificationCommandHandler> _logger;

    public SendAdminNotificationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IBackgroundJobClient backgroundJobClient,
        IFirebaseNotificationService firebaseNotificationService,
        IDateTime dateTimeService,
        ILogger<SendAdminNotificationCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _backgroundJobClient = backgroundJobClient;
        _firebaseNotificationService = firebaseNotificationService;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<NotificationSentResponse> Handle(SendAdminNotificationCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _dateTimeService.UtcNow;
        if (request.ScheduleAt.HasValue && request.ScheduleAt >= utcNow)
        {
            _logger.LogDebug("Admin notification SCHEDULED. Title: {Title}, Time: {ScheduleTime}", request.Title, request.ScheduleAt.Value);

            _backgroundJobClient.Schedule<ScheduleNotificationTask>(
                task => task.ProcessScheduledNotification(request),
                request.ScheduleAt.Value);

            return NotificationSentResponse.Empty;
        }

        if (request.ScheduleAt.HasValue)
        {
            // Notify that the scheduled time is in the past.
            _logger.LogWarning("Admin notification was scheduled in the past. Title: {Title}, Scheduled Time: {ScheduleTime}, Current Time: {CurrentTime}", request.Title, request.ScheduleAt.Value, utcNow);
        }

        IQueryable<User> query = _context.Users
            .AsNoTracking()
            .TagWithContext("NotificationUsers")
            .Where(x => x.Activated);
        
        if (request.AchievementIds?.Count > 0)
        {
            query = query
                .TagWithContext("ByAchievements")
                .Where(x => x.UserAchievements.Any(a => request.AchievementIds.Contains(a.AchievementId)));
        }

        if (request.RoleIds?.Count > 0)
        {
            query = query
                .TagWithContext("ByRoles")
                .Where(x => x.Roles.Any(r => request.RoleIds.Contains(r.RoleId)));
        }

        if (request.UserIds?.Count > 0)
        {
            query = query
                .TagWithContext("ByUsers")
                .Where(x => request.UserIds.Contains(x.Id));
        }

        List<int> targetUserIds = await query
            .Select(x => x.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!targetUserIds.Any())
        {
            _logger.LogWarning("Admin notification: No target users found for the specified criteria. Title: {Title}", request.Title);
            return NotificationSentResponse.Empty;
        }

        _logger.LogInformation("Admin notification: Targeting {UserCount} users. Title: {Title}. Sent by: {AdminUserId}",
            targetUserIds.Count, request.Title, _currentUserService.GetUserId());

        var payload = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            payload.Add("image", request.ImageUrl);
        }

        string currentUserEmail = _currentUserService.GetUserEmail();
        int staffUserId = await _context.Users
            .TagWithContext("GetStaffUserId")
            .AsNoTracking()
            .Where(x => x.Email == currentUserEmail)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var notification = new Notification
        {
            Title = request.Title,
            Message = request.Body,
            Scheduled = request.ScheduleAt,
            NotificationAction = "Send",
            NotificationTag = $"Users:{ListIdsToString(targetUserIds)};" +
                $"AchievementIds:{ListIdsToString(request.AchievementIds)};" +
                $"Roles:{ListIdsToString(request.RoleIds)}",
            NumberOfUsersTargeted = targetUserIds.Count,
            SentByStaffMemberId = staffUserId,
            CreatedUtc = utcNow,
            WasSent = false,
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync(cancellationToken);

        List<int> failedUserIds = [];
        var result = NotificationSentResponse.SendingNotificationTo(targetUserIds.Count);
        foreach (int userId in targetUserIds)
        {
            try
            {
                bool success = await _firebaseNotificationService.SendNotificationAsync(
                    userId,
                    request.Title,
                    request.Body,
                    request.ImageUrl,
                    payload,
                    cancellationToken);

                if (success)
                {
                    _logger.LogDebug("Admin notification SENT for {UserID}. Title: {Title}", userId, request.Title);

                    ++result.NotificationsSent;
                }
                else
                {
                    _logger.LogWarning("Failed to send notification to User ID {UserId}. Title: {Title}", userId, request.Title);

                    failedUserIds.Add(userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing admin notification for User ID {UserId}. Title: {Title}", userId, request.Title);
            }
        }

        notification.NumberOfUsersSent = result.NotificationsSent;
        notification.WasSent = true;
        notification.SentOn = _dateTimeService.UtcNow;
        notification.FailedUserIds = failedUserIds;
        notification.HasError = failedUserIds.Any();

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    private static string ListIdsToString(IEnumerable<int>? ids)
        => ids != null && ids.Any() ? string.Join(',', ids) : "";
}
