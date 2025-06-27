using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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

    /// <summary>
    /// This is used for scheduled notifications once the message is already created.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? NotificationId { get; set; }
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
        Notification? notification;
        var utcNow = _dateTimeService.UtcNow;
        var targetUsersQuery = GetTargetUserIdsQuery(request);
        if (request.ScheduleAt.HasValue && request.ScheduleAt >= utcNow)
        {
            _logger.LogDebug("Admin notification SCHEDULED. Title: {Title}, Time: {ScheduleTime}", request.Title, request.ScheduleAt.Value);

            int staffUserId = await GetStaffUserId(cancellationToken);
            notification = new()
            {
                Title = request.Title,
                Message = request.Body,
                Scheduled = request.ScheduleAt,
                NotificationAction = "Send",
                NotificationTag = $"Users:{ListIdsToString(request.UserIds)};" +
                $"AchievementIds:{ListIdsToString(request.AchievementIds)};" +
                $"Roles:{ListIdsToString(request.RoleIds)}",
                SentByStaffMemberId = staffUserId,
                CreatedUtc = utcNow,
                WasSent = false,
                NumberOfUsersTargeted = await targetUsersQuery.CountAsync(cancellationToken)
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync(cancellationToken);

            request.NotificationId = notification.Id;
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

        List<int> targetUserIds = await targetUsersQuery.ToListAsync(cancellationToken);
        if (!targetUserIds.Any())
        {
            _logger.LogWarning("Admin notification: No target users found for the specified criteria. Title: {Title}", request.Title);
        }

        _logger.LogInformation("Admin notification: Targeting {UserCount} users. Title: {Title}. Sent by: {AdminUserId}",
            targetUserIds.Count, request.Title, _currentUserService.GetUserId());

        var payload = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            payload.Add("image", request.ImageUrl);
        }

        if (request.NotificationId is null)
        {
            int staffUserId = await GetStaffUserId(cancellationToken);
            notification = new()
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
        }
        else
        {
            notification = await _context.Notifications
                .TagWithContext("GetNotificationById")
                .FirstOrDefaultAsync(x => x.Id == request.NotificationId.Value, cancellationToken);

            if (notification == null)
            {
                _logger.LogError("Notification with ID {NotificationId} not found.", request.NotificationId);
                throw new InvalidOperationException($"Notification with ID {request.NotificationId} not found.");
            }

            if (notification.WasSent)
            {
                _logger.LogWarning("Notification with ID {NotificationId} has already been sent. Skipping.", request.NotificationId);
                return NotificationSentResponse.Empty;
            }

            notification.NotificationTag = $"Users:{ListIdsToString(targetUserIds)};" +
                $"AchievementIds:{ListIdsToString(request.AchievementIds)};" +
                $"Roles:{ListIdsToString(request.RoleIds)}";
            notification.NumberOfUsersTargeted = targetUserIds.Count;
        }

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

    private IQueryable<int> GetTargetUserIdsQuery(SendAdminNotificationCommand request)
    {
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

        return query
            .Select(x => x.Id)
            .Distinct();
    }

    private async Task<int> GetStaffUserId(CancellationToken cancellationToken)
    {
        string currentUserEmail = _currentUserService.GetUserEmail();
        return await _context.Users
            .TagWithContext("GetStaffUserId")
            .AsNoTracking()
            .Where(x => x.Email == currentUserEmail)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string ListIdsToString(IEnumerable<int>? ids)
        => ids != null && ids.Any() ? string.Join(',', ids) : "";
}
