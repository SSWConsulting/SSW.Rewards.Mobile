using System.Net.Mail;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.Rewards.Shared.DTOs.Users;
using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Services;

public class UserService : IUserService, IRolesService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly string _staffSMTPDomain;

    public UserService(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IMapper mapper,
        IOptions<UserServiceOptions> options,
        ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;

        _staffSMTPDomain = options.Value.StaffSmtpDomain;
    }

    public int AddRole(Role role)
    {
        return AddRole(role, CancellationToken.None).Result;
    }

    public async Task<int> AddRole(Role role, CancellationToken cancellationToken)
    {
        _dbContext.Roles.Add(role);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return role.Id;
    }

    public void AddUserRole(User user, Role role)
    {
        AddUserRole(user, role, CancellationToken.None).RunSynchronously();
    }

    public async Task AddUserRole(User user, Role role, CancellationToken cancellationToken)
    {
        user.Roles.Add(new UserRole
        {
            Role = role
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public int CreateUser(User user)
    {
        return CreateUser(user, CancellationToken.None).Result;
    }

    public async Task<int> CreateUser(User newUser, CancellationToken cancellationToken)
    {
        var currentUser = await _dbContext.Users.Where(u => u.Email == newUser.Email).FirstOrDefaultAsync(cancellationToken);

        if (currentUser != null)
        {
            _logger.LogWarning("User with {email} already exists", newUser.Email);
            return currentUser.Id;
        }

        var userRole = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == "User");

        newUser.Roles.Add(new UserRole { Role = userRole });

        var userEmail = new MailAddress(newUser.Email);

        if (userEmail.Host == _staffSMTPDomain)
        {
            var staffRole = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == "Staff");

            newUser.Roles.Add(new UserRole { Role = staffRole });
        }

        var unclaimedAchievements = await _dbContext.UnclaimedAchievements
            .Where(ua => ua.EmailAddress.ToLower() == newUser.Email.ToLower())
            .ToListAsync();

        if (unclaimedAchievements.Any())
        {
            foreach (var achievement in unclaimedAchievements)
            {
                newUser.UserAchievements.Add(new UserAchievement
                {
                    Achievement = achievement.Achievement,
                    AwardedAt = DateTime.UtcNow,
                    CreatedUtc = DateTime.UtcNow
                });

                _dbContext.UnclaimedAchievements.Remove(achievement);
            }
        }

        _dbContext.Users.Add(newUser);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return newUser.Id;
    }

    public async Task<int> GetUserId(string email, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException("no email provided");
        email = email.Trim().ToLower();
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);
        if (user == null)
            throw new NotFoundException("No user found");
        return user.Id;
    }

    public CurrentUserDto GetCurrentUser()
    {
        return GetCurrentUser(CancellationToken.None).Result;
    }

    public async Task<CurrentUserDto> GetCurrentUser(CancellationToken cancellationToken)
    {
        string currentUserEmail = _currentUserService.GetUserEmail();

        var user = await _dbContext.Users
                .Where(u => u.Email == currentUserEmail)
                .Include(u => u.UserAchievements)
                    .ThenInclude(ua => ua.Achievement)
                .Include(u => u.UserRewards)
                    .ThenInclude(ur => ur.Reward)
                .SingleOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), currentUserEmail);
        }

        if (!user.Activated)
        {
            user.Activated = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return _mapper.Map<CurrentUserDto>(user);
    }

    public IEnumerable<Role> GetCurrentUserRoles()
    {
        return GetCurrentUserRoles(CancellationToken.None).Result;
    }

    public async Task<IEnumerable<Role>> GetCurrentUserRoles(CancellationToken cancellationToken)
    {
        string email = _currentUserService.GetUserEmail();
        return await _dbContext.UserRoles
            .TagWith($"{nameof(UserService)}-{nameof(GetCurrentUserRoles)}")
            .Where(x => x.User.Email == email)
            .Select(x => x.Role)
            .ToListAsync(cancellationToken);
    }

    public string GetStaffQRCode(string emailAddress)
    {
        return GetStaffQRCode(emailAddress, CancellationToken.None).Result;
    }

    public async Task<string> GetStaffQRCode(string emailAddress, CancellationToken cancellationToken)
    {
        var code = await _dbContext.StaffMembers
                .AsNoTracking()
                .Include(s => s.StaffAchievement)
                .Where(s => s.Email == emailAddress)
                .Select(s => s.StaffAchievement!.Code)
                .SingleOrDefaultAsync(cancellationToken);

        return code ?? string.Empty;
    }

    public UserProfileDto GetUser(int userId)
    {
        return GetUser(userId, CancellationToken.None).Result;
    }

    public async Task<UserProfileDto> GetUser(int userId, CancellationToken cancellationToken)
    {
        var vm = await _dbContext.Users
                .Where(u => u.Id == userId)
                .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

        if (vm == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var userAchievements = await _dbContext.UserAchievements
                                    .Include(ua => ua.Achievement)
                                    .Where(u => u.UserId == userId)
                                    .ProjectTo<UserAchievementDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync();

        var userRewards = await _dbContext.UserRewards
                                    .Include(ur => ur.Reward)
                                    .Where(u => u.UserId == userId)
                                    .ProjectTo<UserRewardDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync();

        vm.Achievements = userAchievements;
        vm.Rewards = userRewards;

        var points = userAchievements.Sum(a => a.AchievementValue);
        var spent = userRewards.Sum(r => r.RewardCost);
        vm.Points = points;
        vm.Balance = points - spent;

        return vm;
    }
    
    public UsersViewModel GetUsers()
    {
        return GetUsers(CancellationToken.None).Result;
    }

    public async Task<UsersViewModel> GetUsers(CancellationToken cancellationToken)
    {
        var vm = await _dbContext.Users
            .Include(x => x.Roles)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        if (vm == null)
        {
            throw new NotFoundException(nameof(User));
        }

        return new UsersViewModel { Users = vm };
    }
    
    public UserAchievementsViewModel GetUserAchievements(int userId)
    {
        return GetUserAchievements(userId, CancellationToken.None).Result;
    }

    public async Task<UserAchievementsViewModel> GetUserAchievements(int userId, CancellationToken cancellationToken)
    {
        var userAchievements = await _dbContext.UserAchievements
            .Where(u => u.UserId == userId)
            .ProjectTo<UserAchievementDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new UserAchievementsViewModel
        {
            UserId = userId,
            Points = userAchievements
                .Where(ua => ua.Complete)
                .Sum(ua => ua.AchievementValue),
            UserAchievements = userAchievements
        };
    }

    public UserRewardsViewModel GetUserRewards(int userId)
    {
        return GetUserRewards(userId, CancellationToken.None).Result;
    }

    public async Task<UserRewardsViewModel> GetUserRewards(int userId, CancellationToken cancellationToken)
    {
        // This throws NRE. Original code below for now, but this should work.
        // TODO: figure out why this doesn't work.
        //return await _dbContext.Users.Where(u => u.Id == userId)
        //            .ProjectTo<UserRewardsViewModel>(_mapper.ConfigurationProvider)
        //            .FirstOrDefaultAsync(cancellationToken);

        var rewards = await _dbContext.Rewards.ToListAsync(cancellationToken);
        var userRewards = await _dbContext.UserRewards
            .Where(ur => ur.UserId == userId)
            .ProjectTo<UserRewardDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Currently using in-memory join because the expected returned records are very low (max 10 or so)
        var vm = new List<UserRewardDto>();

        vm.AddRange(userRewards);

        return new UserRewardsViewModel
        {
            UserId = userId,
            UserRewards = vm
        };
    }
    
    public UserPendingRedemptionsViewModel GetUserPendingRedemptions(int userId)
    {
        return GetUserPendingRedemptions(userId, CancellationToken.None).Result;
    }
    
    public async Task<UserPendingRedemptionsViewModel> GetUserPendingRedemptions(int userId, CancellationToken cancellationToken)
    {
        var pendingRedemptions = await _dbContext.PendingRedemptions
            .Where(x => x.User.Id == userId && !x.Completed && !x.CancelledByAdmin && !x.CancelledByAdmin)
            .ProjectTo<UserPendingRedemptionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new UserPendingRedemptionsViewModel { UserId = userId, PendingRedemptions = pendingRedemptions };
    }

    public IEnumerable<Role> GetUserRoles(int userId)
    {
        return GetUserRoles(userId, CancellationToken.None).Result;
    }

    public async Task<IEnumerable<Role>> GetUserRoles(int userId, CancellationToken cancellationToken)
    {
        return await _dbContext.UserRoles
            .AsNoTracking()
            .TagWith($"{nameof(UserService)}-{nameof(GetUserRoles)}")
            .Where(x => x.User.Id == userId)
            .Select(x => x.Role)
            .ToListAsync(cancellationToken);
    }

    public void RemoveUserRole(int userId, int roleId)
    {
        RemoveUserRole(userId, roleId, CancellationToken.None).RunSynchronously();
    }

    public async Task RemoveUserRole(int userId, int roleId, CancellationToken cancellationToken)
    {
        var userToEdit = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var roleToRemove = await _dbContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

        userToEdit.Roles.Remove(roleToRemove);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void UpdateUser(int userId, User user)
    {
        UpdateUser(userId, user, CancellationToken.None);
    }

    public Task UpdateUser(int UserId, User user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
