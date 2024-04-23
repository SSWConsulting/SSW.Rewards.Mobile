namespace SSW.Rewards.Application.Users.Commands.UpsertUserSocialMediaId;
public class UpsertUserSocialMediaIdCommand : IRequest<bool>
{
    public int AchievementId { get; set; }
    public string SocialMediaUserId { get; set; } = string.Empty;
}

public sealed class UpsertSocialMediaUserIdHandler : IRequestHandler<UpsertUserSocialMediaIdCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTime _dateTimeService;

    public UpsertSocialMediaUserIdHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTime dateTimeService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeService = dateTimeService;
    }

    public async Task<bool> Handle(UpsertUserSocialMediaIdCommand request, CancellationToken cancellationToken)
    {
        bool isInsert = false;

        var platform = await _context.SocialMediaPlatforms
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.AchievementId == request.AchievementId, cancellationToken);

        if (platform == null)
            throw new ArgumentException($"Social Media Platform not found");

        int currentUserId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);
        var record = await _context.UserSocialMediaIds
                                    .FirstOrDefaultAsync(x =>
                                                x.UserId == currentUserId
                                            &&  x.SocialMediaPlatformId == platform.Id, cancellationToken);
        if (record == null)
        {
            isInsert = true;
            record = new UserSocialMediaId
            {
                SocialMediaPlatformId = platform.Id,
                UserId                = currentUserId,
                SocialMediaUserId     = request.SocialMediaUserId,
                CreatedUtc            = _dateTimeService.Now
            };
            _context.UserSocialMediaIds.Add(record);
        }
        else
        {
            record.SocialMediaUserId = request.SocialMediaUserId;
            _context.UserSocialMediaIds.Update(record);
        }
        await _context.SaveChangesAsync(cancellationToken);

        return isInsert;
    }
}

