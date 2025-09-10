using SSW.Rewards.Application.Achievements.Notifications;
using SSW.Rewards.Application.System.Commands.Common;
using SSW.Rewards.Shared.DTOs.Achievements;
using Microsoft.EntityFrameworkCore.Storage;

namespace SSW.Rewards.Application.Achievements.Command.PostAchievement;

public class PostAchievementCommand : IRequest<ClaimAchievementResult>
{
    public string Code { get; set; }
}

public class PostAchievementCommandHandler : IRequestHandler<PostAchievementCommand, ClaimAchievementResult>
{
    private readonly IUserService _userService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public PostAchievementCommandHandler(
        IUserService userService,
        IApplicationDbContext context,
        IMapper mapper,
        IMediator mediator)
    {
        _userService = userService;
        _context = context;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<ClaimAchievementResult> Handle(PostAchievementCommand request, CancellationToken cancellationToken)
    {
        var requestedAchievement = await _context.Achievements
           .AsNoTracking()
           .TagWithContext("GetRelevantAchievement")
           .Where(a => a.Code == request.Code)
           .FirstOrDefaultAsync(cancellationToken);

        if (requestedAchievement == null)
        {
            return new ClaimAchievementResult
            {
                status = ClaimAchievementStatus.NotFound
            };
        }

        var achievementModel = _mapper.Map<AchievementDto>(requestedAchievement);

        var userId = await _userService.GetCurrentUserId(cancellationToken);

        var userAchievements = await _context.UserAchievements
            .AsNoTracking()
            .TagWithContext("GetAllUserAchievement")
            .Include(x => x.Achievement)
            .Where(ua => ua.UserId == userId)
            .ToListAsync(cancellationToken);

        // If multiscan is disabled and user already has this achievement, return duplicate
        if (!requestedAchievement.IsMultiscanEnabled && userAchievements.Any(ua => ua.AchievementId == requestedAchievement.Id))
        {
            return new ClaimAchievementResult
            {
                viewModel = achievementModel,
                status = ClaimAchievementStatus.Duplicate,
            };
        }

        // Check for milestones specific to this achievement
        if (requestedAchievement.Type == AchievementType.Scanned)
        {
            var scannedUser = await _context.Users
                .AsNoTracking()
                .TagWithContext("GetScannedUserByAchievementId")
                .Where(u => u.AchievementId == requestedAchievement.Id)
                .Select(x => new { x.Id })
                .FirstOrDefaultAsync(cancellationToken);

            if (scannedUser == null)
            {
                var staffMember = await _context.StaffMembers
                    .AsNoTracking()
                    .TagWithContext("GetScannedStaffMember")
                    .Where(s => s.StaffAchievement != null && s.StaffAchievement!.Id == requestedAchievement.Id)
                    .Select(x => new { x.Email })
                    .FirstOrDefaultAsync(cancellationToken);

                if (staffMember != null)
                {
                    var staffUser = await _context.Users
                        .AsNoTracking()
                        .TagWithContext("GetScannedUserByEmail")
                        .Where(u => u.Email == staffMember.Email)
                        .Select(x => new { x.Id })
                        .FirstOrDefaultAsync(cancellationToken);

                    achievementModel.UserId = staffUser?.Id;
                }
            }
            else
            {
                achievementModel.UserId = scannedUser.Id;
            }

            if (achievementModel.UserId != null && !userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.MeetSSW))
            {
                var meetAchievement = await _context.Achievements
                    .AsNoTracking()
                    .TagWithContext("GetMilestone-MeetSSW")
                    .Where(a => a.Name == MilestoneAchievements.MeetSSW)
                    .Select(x => new { x.Id })
                    .FirstOrDefaultAsync(cancellationToken);

                if (meetAchievement != null)
                {
                    var userMeetAchievement = new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = meetAchievement.Id
                    };

                    _context.UserAchievements.Add(userMeetAchievement);
                }
            }
        }

        var userAchievement = new UserAchievement
        {
            UserId = userId,
            AchievementId = requestedAchievement.Id
        };

        // For achievements with multiscan disabled, use a transaction to prevent race conditions
        if (!requestedAchievement.IsMultiscanEnabled)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            
            try
            {
                // Check one more time within the serializable transaction to prevent race conditions
                var existingUserAchievement = await _context.UserAchievements
                    .TagWithContext("TransactionDuplicateCheck")
                    .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == requestedAchievement.Id, cancellationToken);
                
                if (existingUserAchievement != null)
                {
                    return new ClaimAchievementResult
                    {
                        viewModel = achievementModel,
                        status = ClaimAchievementStatus.Duplicate,
                    };
                }

                _context.UserAchievements.Add(userAchievement);

                if (requestedAchievement.Type == AchievementType.Attended)
                {
                    int? milestoneAchievementId = await (requestedAchievement.Icon switch
                    {
                        Icons.Puzzle when !userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendUG) 
                            => GetAchievementId(MilestoneAchievements.AttendUG, cancellationToken),

                        Icons.Lightbulb when !userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendHackday) 
                            => GetAchievementId(MilestoneAchievements.AttendHackday, cancellationToken),

                        Icons.Lightning when !userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendSuperpowers) 
                            => GetAchievementId(MilestoneAchievements.AttendSuperpowers, cancellationToken),

                        Icons.Certificate when !userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendWorkshop) 
                            => GetAchievementId(MilestoneAchievements.AttendWorkshop, cancellationToken),

                        _ => Task.FromResult<int?>(null)
                    });

                    if (milestoneAchievementId.HasValue)
                    {
                        _context.UserAchievements.Add(new UserAchievement
                        {
                            UserId = userId,
                            AchievementId = milestoneAchievementId.Value
                        });
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        else
        {
            // For multiscan enabled achievements, use the normal flow
            _context.UserAchievements.Add(userAchievement);

            if (requestedAchievement.Type == AchievementType.Attended)
            {
                int? milestoneAchievementId = await (requestedAchievement.Icon switch
                {
                    Icons.Puzzle when !userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendUG) 
                        => GetAchievementId(MilestoneAchievements.AttendUG, cancellationToken),

                    Icons.Lightbulb when !userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendHackday) 
                        => GetAchievementId(MilestoneAchievements.AttendHackday, cancellationToken),

                    Icons.Lightning when !userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendSuperpowers) 
                        => GetAchievementId(MilestoneAchievements.AttendSuperpowers, cancellationToken),

                    Icons.Certificate when !userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendWorkshop) 
                        => GetAchievementId(MilestoneAchievements.AttendWorkshop, cancellationToken),

                    _ => Task.FromResult<int?>(null)
                });

                if (milestoneAchievementId.HasValue)
                {
                    _context.UserAchievements.Add(new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = milestoneAchievementId.Value
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        // Check for any other milestone achievements that the user may have reached.
        await _mediator.Publish(new UserMilestoneAchievementCheckRequested { UserId = userId }, cancellationToken);

        return new ClaimAchievementResult
        {
            viewModel = achievementModel,
            status = ClaimAchievementStatus.Claimed
        };
    }

    private async Task<int?> GetAchievementId(string achievementName, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements
            .AsNoTracking()
            .TagWithContext($"GetMilestone-{achievementName}")
            .Where(a => a.Name == achievementName)
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync(cancellationToken);

        return achievement?.Id;
    }
}
