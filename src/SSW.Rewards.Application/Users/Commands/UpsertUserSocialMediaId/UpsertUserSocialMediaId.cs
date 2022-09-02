namespace SSW.Rewards.Application.Users.Commands.UpsertUserSocialMediaId;
public class UpsertUserSocialMediaId : IRequest<bool>
{
    public int AchievementId { get; set; }
    public string SocialMediaPlatformUserId { get; set; } = string.Empty;
}

public sealed class UpsertSocialMediaUserIdHandler : IRequestHandler<UpsertUserSocialMediaId, bool>
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

    public async Task<bool> Handle(UpsertUserSocialMediaId request, CancellationToken cancellationToken)
    {
        bool isInsert = false;

        var platform = await _context.SocialMediaPlatforms
            .FirstOrDefaultAsync(p => p.AchievementId == request.AchievementId, cancellationToken);

        if (platform == null)
            throw new ArgumentException($"Social Media Platform not found");

        int currentUserId = await this._userService.GetUserId(this._currentUserService.GetUserEmail());
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
                SocialMediaUserId     = request.SocialMediaPlatformUserId,
                CreatedUtc            = _dateTimeService.Now  
            };
            _context.UserSocialMediaIds.Add(record);
        }
        else
        {
            record.SocialMediaUserId = request.SocialMediaPlatformUserId;
            _context.UserSocialMediaIds.Update(record);
        }
        await _context.SaveChangesAsync(cancellationToken);

        return isInsert;
    }
}

