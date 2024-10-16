using SSW.Rewards.Shared.DTOs.Achievements;
using SSW.Rewards.Application.Achievements.Queries.Common;
using SSW.Rewards.Application.System.Commands.Common;

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

    public PostAchievementCommandHandler(
        IUserService UserService,
        IApplicationDbContext context,
        IMapper mapper)
    {
        _userService = UserService;
        _context = context;
        _mapper = mapper;
    }

    public async Task<ClaimAchievementResult> Handle(PostAchievementCommand request, CancellationToken cancellationToken)
    {
        var requestedAchievement = await _context
           .Achievements
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

        var user = await _userService.GetCurrentUser(cancellationToken);

        var userAchievements = await _context
            .UserAchievements
            .Where(ua => ua.UserId == user.Id)
            .ToListAsync(cancellationToken);
        
        // check for milestone achievements
        if (requestedAchievement.Type == AchievementType.Scanned)
        {
            // TODO: Re-enable when fixed
            // var scannedUser = await _context.Users
            //     .FirstOrDefaultAsync(u => u.AchievementId == requestedAchievement.Id, cancellationToken);
            //
            // if (scannedUser == null)
            // {
            //     var staffMember = await _context.StaffMembers
            //         .Include(s => s.StaffAchievement)
            //         .Where(s => s.StaffAchievement != null)
            //         .FirstOrDefaultAsync(s => s.StaffAchievement!.Id == requestedAchievement.Id, cancellationToken);
            //
            //     achievementModel.UserId = staffMember?.Id;
            // }
            // else
            // {
            //     achievementModel.UserId = scannedUser.Id;
            // }
            
            if (!userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.MeetSSW))
            {
                var meetAchievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.MeetSSW, cancellationToken);

                var userMeetAchievement = new UserAchievement
                {
                    UserId = user.Id,
                    Achievement = meetAchievement
                };

                _context.UserAchievements.Add(userMeetAchievement);
            }
        }

        if (userAchievements.Any(ua => ua.Achievement == requestedAchievement && !requestedAchievement.IsMultiscanEnabled))
        {
            return new ClaimAchievementResult
            {
                viewModel = achievementModel,
                status = ClaimAchievementStatus.Duplicate,
            };
        }

        var userAchievement = new UserAchievement
        {
            UserId = user.Id,
            AchievementId = requestedAchievement.Id
        };

        _context.UserAchievements.Add(userAchievement);

        if (requestedAchievement.Type == AchievementType.Attended)
        {
            var milestoneAchievement = new UserAchievement { UserId = user.Id };

            // UG = puzzle, Hackday = lightbulb, Superpowers = lightning, Workshop = certificate
            switch (requestedAchievement.Icon)
            {
                case Icons.Puzzle:

                    if (!userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendUG))
                    {
                        var ugAchievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.AttendUG, cancellationToken);
                        milestoneAchievement.Achievement = ugAchievement;
                    }
                    break;

                case Icons.Lightbulb:

                    if (!userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendHackday))
                    {
                        var hdAchievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.AttendHackday, cancellationToken);
                        milestoneAchievement.Achievement = hdAchievement;
                    }
                    break;

                case Icons.Lightning:

                    if (!userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendSuperpowers))
                    {
                        var spAchievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.AttendSuperpowers, cancellationToken);
                        milestoneAchievement.Achievement = spAchievement;
                    }
                    break;

                case Icons.Certificate:

                    if (!userAchievements.Any(ua => ua.Achievement.Name == MilestoneAchievements.AttendWorkshop))
                    {
                        var wsAchievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.AttendWorkshop, cancellationToken);
                        milestoneAchievement.Achievement = wsAchievement;
                    }
                    break;

                default:
                    break;
            }

            if (milestoneAchievement.Achievement != null)
            {
                _context.UserAchievements.Add(milestoneAchievement);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ClaimAchievementResult
        {
            viewModel = achievementModel,
            status = ClaimAchievementStatus.Claimed
        };
    }
}
