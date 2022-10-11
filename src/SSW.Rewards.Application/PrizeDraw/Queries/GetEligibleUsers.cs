using System.Security.Cryptography;
using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Extensions;
using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Application.Leaderboard.Queries.GetFilteredLeaderboardList;
using SSW.Rewards.Application.Rewards.Commands.UpdateReward;

namespace SSW.Rewards.Application.PrizeDraw.Queries;

public class EligibleUserViewModel : IMapFrom<User>
{
    public int? UserId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public int TotalPoints { get; set; }

    public int PointsClaimed { get; set; }

    public int PointsToday { get; set; }
    public int PointsThisWeek { get; set; }

    public int PointsThisMonth { get; set; }

    public int PointsThisYear { get; set; }

    public int Balance { get { return TotalPoints - PointsClaimed; } set { _ = value; } }

    public void Mapping(Profile profile)
    {
        var start = DateTime.Now.FirstDayOfWeek();
        var end = DateTime.Now.FirstDayOfWeek().AddDays(-7);

        profile.CreateMap<User, EligibleUserViewModel>()
                .ForMember(dst => dst.Balance, opt => opt.Ignore())
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dst => dst.TotalPoints, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.PointsClaimed, opt => opt.MapFrom(src => src.UserRewards.Sum(ur => ur.Reward.Cost)))

                // TODO:    Using DateTime.Now here presents instability for testing the queries dependent
                //          on this DTO. As discussed with williamliebenberg@ssw.com.au, we will accept
                //          this tech debt for now and investigate a better approach in the future. See
                //          https://github.com/SSWConsulting/SSW.Rewards.API/issues/7
                .ForMember(dst => dst.PointsThisYear, opt => opt.MapFrom(src => src.UserAchievements
                    .Where(ua => ua.AwardedAt.Year == DateTime.Now.Year)
                    .Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.PointsThisMonth, opt => opt.MapFrom(src => src.UserAchievements
                    .Where(ua => ua.AwardedAt.Year == DateTime.Now.Year && ua.AwardedAt.Month == DateTime.Now.Month)
                    .Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.PointsToday, opt => opt.MapFrom(src => src.UserAchievements
                    .Where(ua => ua.AwardedAt.Year == DateTime.Now.Year && ua.AwardedAt.Month == DateTime.Now.Month && ua.AwardedAt.Day == DateTime.Now.Day)
                    .Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.PointsThisWeek, opt => opt.MapFrom(src => src.UserAchievements
                    .Where(ua => start <= ua.AwardedAt && ua.AwardedAt <= end)
                    .Sum(ua => ua.Achievement.Value)));
    }
}

public class EligibleUsersViewModel
{
    public int RewardId { get; set; }
    public string? RewardCode { get; set; }
    public IEnumerable<EligibleUserViewModel> EligibleUsers { get; set; } = new List<EligibleUserViewModel>();
}

public class GetEligibleUsers : IRequest<EligibleUsersViewModel>
{
    public int RewardId { get; set; }
    public LeaderboardFilter Filter { get; set; }
    public bool BalanceRequired { get; set; }
    public bool FilterStaff { get; set; }
}

public class GetEligibleUsersValidator : AbstractValidator<GetEligibleUsers>
{
    public GetEligibleUsersValidator()
    {
        RuleFor(r => r.RewardId)
            .Must(r => r > 0);

        //RuleFor(r => r.Filter)
        //    .Must(BeValidFilter)
        //    .WithMessage("Invalid filter");
    }

    //public bool BeValidFilter(LeaderboardFilter filter)
    //{
    //    return 
    //        filter == LeaderboardFilter.Forever || 
    //        filter == LeaderboardFilter.Today ||
    //        filter == LeaderboardFilter.ThisMonth ||
    //        filter == LeaderboardFilter.ThisYear;
    //}
}

public class GetEligibleUsersHandler : IRequestHandler<GetEligibleUsers, EligibleUsersViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDateTime _dateTime;

    public GetEligibleUsersHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IDateTime dateTime)
    {
        _context = context;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<EligibleUsersViewModel> Handle(GetEligibleUsers request, CancellationToken cancellationToken)
    {
        var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.Id == request.RewardId, cancellationToken);
        if (reward == null)
        {
            throw new NotFoundException(nameof(UpdateRewardCommand), request.RewardId);
        }

        // find all the activated users with enough points in the (today/month/year/forever) leaderboard to claim the specific reward 
        var eligibleUsers = _context.Users.Where(u => u.Activated == true);

        if (request.Filter == LeaderboardFilter.ThisYear)
        {
            eligibleUsers = eligibleUsers.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year));
        }
        else if (request.Filter == LeaderboardFilter.ThisMonth)
        {
            eligibleUsers = eligibleUsers.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year && a.AwardedAt.Month == _dateTime.Now.Month));
        }
        else if (request.Filter == LeaderboardFilter.Today)
        {
            eligibleUsers = eligibleUsers.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year && a.AwardedAt.Month == _dateTime.Now.Month && a.AwardedAt.Day == _dateTime.Now.Day));
        }
        else if (request.Filter == LeaderboardFilter.ThisWeek)
        {
            var start = DateTime.Now.FirstDayOfWeek();
            var end = DateTime.Now.FirstDayOfWeek().AddDays(-7);
            // TODO: Find a better way - EF Can't translate our extension method -- so writing the date range comparison directly in linq for now
            eligibleUsers = eligibleUsers.Where(u => u.UserAchievements.Any(a => start <= a.AwardedAt && a.AwardedAt <= end ));
        }
        else if (request.Filter == LeaderboardFilter.Forever)
        {
            // no action
        }

        eligibleUsers = eligibleUsers
            .Include(u => u.UserRewards).ThenInclude(ur => ur.Reward)
            .Include(u => u.UserAchievements).ThenInclude(u => u.Achievement);

        if(request.BalanceRequired)
        {
            eligibleUsers = eligibleUsers.Where(u =>
                // Eligible = (Total Points - Points already Claimed):RemainingPoints >= Reward.Cost
                (u.UserAchievements.Sum(ua => ua.Achievement.Value) - u.UserRewards.Sum(ur => ur.Reward.Cost)) >= reward.Cost);
        }
        
        var users = await eligibleUsers
            .ProjectTo<EligibleUserViewModel>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // filter out any @ssw.com.au users (.ends with didn't translate with EF Core :(
        if (request.FilterStaff)
        {
            users = users.Where(u => (u.Email != null && !u.Email.EndsWith("@ssw.com.au", StringComparison.OrdinalIgnoreCase))).ToList();
        }

        EligibleUsersViewModel vm = new()
        {
            RewardId = request.RewardId,
            RewardCode = reward.Code,
            EligibleUsers = users
        };

        vm.EligibleUsers = vm.EligibleUsers.OrderByDescending(u => u.Balance);

        return vm;
    }
}