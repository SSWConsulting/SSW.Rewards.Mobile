using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Services;

namespace SSW.Rewards.Application.Notifications.Commands;

public class SendAdminNotificationCommand : IRequest
{
    [Required]
    public DeliveryOption DeliveryOption { get; set; }

    public DateTimeOffset? ScheduleAt { get; set; }

    public int? AchievementId { get; set; }

    public int? UserId { get; set; }

    public string? Role { get; set; } // e.g., "Admin", "Staff", "User"

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Body { get; set; } = string.Empty;

    [Url]
    public string? ImageUrl { get; set; }
}

public enum DeliveryOption
{
    Now,
    Schedule
}

public class SendAdminNotificationCommandHandler : IRequestHandler<SendAdminNotificationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IFirebaseNotificationService _firebaseNotificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SendAdminNotificationCommandHandler> _logger;

    public SendAdminNotificationCommandHandler(
        IApplicationDbContext context,
        IFirebaseNotificationService firebaseNotificationService,
        ICurrentUserService currentUserService,
        ILogger<SendAdminNotificationCommandHandler> logger)
    {
        _context = context;
        _firebaseNotificationService = firebaseNotificationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(SendAdminNotificationCommand request, CancellationToken cancellationToken)
    {
        var targetUserIds = new List<int>();

        if (request.UserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId.Value && u.Activated, cancellationToken);
            if (userExists)
            {
                targetUserIds.Add(request.UserId.Value);
            }
            else
            {
                _logger.LogWarning("Admin notification: Targeted User ID {UserId} not found or not activated.", request.UserId.Value);
                return;
            }
        }
        else if (request.AchievementId.HasValue)
        {
            targetUserIds = await _context.UserAchievements
                .Where(ua => ua.AchievementId == request.AchievementId.Value)
                .Select(ua => ua.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.Role))
        {
            targetUserIds = await _context.UserRoles
                .Where(ur => ur.Role.Name == request.Role && ur.User.Activated)
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
        else
        {
            targetUserIds = await _context.Users
                .Where(u => u.Activated)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
            _logger.LogInformation("Admin notification: No specific target, preparing to send to all {UserCount} activated users.", targetUserIds.Count);
        }

        if (!targetUserIds.Any())
        {
            _logger.LogWarning("Admin notification: No target users found for the specified criteria. Title: {Title}", request.Title);
            return;
        }

        _logger.LogInformation("Admin notification: Targeting {UserCount} users. Title: {Title}. Sent by: {AdminUserId}",
            targetUserIds.Count, request.Title, _currentUserService.GetUserId());

        var payload = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            payload.Add("image", request.ImageUrl);
        }

        foreach (var userId in targetUserIds)
        {
            try
            {
                if (request.DeliveryOption == DeliveryOption.Schedule)
                {
                    if (!request.ScheduleAt.HasValue)
                    {
                        _logger.LogError("Admin notification: Scheduling requested but ScheduleAt is null. UserID: {UserId}, Title: {Title}", userId, request.Title);
                        continue;
                    }
                    _firebaseNotificationService.ScheduleNotification(
                        userId,
                        request.Title,
                        request.Body,
                        payload,
                        request.ScheduleAt.Value);
                    
                    _logger.LogDebug("Admin notification SCHEDULED for User ID {UserId}. Title: {Title}, Time: {ScheduleTime}", userId, request.Title, request.ScheduleAt.Value);
                }
                else // DeliveryOption.Now
                {
                    await _firebaseNotificationService.SendNotificationAsync(
                        userId,
                        request.Title,
                        request.Body,
                        payload,
                        cancellationToken);
                    _logger.LogDebug("Admin notification SENT to User ID {UserId}. Title: {Title}", userId, request.Title);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing admin notification for User ID {UserId}. Title: {Title}", userId, request.Title);
            }
        }
    }
}
