using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffList;

public class GetStaffListQuery : IRequest<StaffListViewModel> { }

public sealed class Handler : IRequestHandler<GetStaffListQuery, StaffListViewModel>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserService _userService;

    public Handler(
        IMapper mapper,
        IApplicationDbContext dbContext,
        IUserService userService)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<StaffListViewModel> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var staffDtos = await _dbContext.StaffMembers
            .IgnoreQueryFilters()
            .TagWithContext()
            .ProjectTo<StaffMemberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        int userId = await _userService.GetCurrentUserId(cancellationToken);
        List<int> completedAchievements = await _dbContext.UserAchievements
            .TagWithContext()
            .Where(x => x.UserId == userId)
            .Select(x => x.AchievementId)
            .ToListAsync(cancellationToken);

        foreach (var dto in staffDtos.Where(x => x.StaffAchievement?.Id != null))
        {
            if (completedAchievements.Contains(dto.StaffAchievement!.Id))
            {
                dto.Scanned = true;
            }
        }

        return new StaffListViewModel
        {
            Staff = staffDtos
        };
    }
}
