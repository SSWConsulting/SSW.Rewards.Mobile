using System.Net.Mail;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Helpers;
using SSW.Rewards.Application.Leaderboard;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Services;

public class UserService : IUserService, IRolesService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly ILeaderboardService _leaderboardService;
    private readonly string _staffSmtpDomain;

    public UserService(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IMapper mapper,
        IOptions<UserServiceOptions> options,
        ILogger<UserService> logger,
        ILeaderboardService leaderboardService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
        _leaderboardService = leaderboardService;

        _staffSmtpDomain = options.Value.StaffSmtpDomain;
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

    public User CreateUser(User user)
    {
        return CreateUser(user, CancellationToken.None).Result;
    }

    public async Task<User> CreateUser(User newUser, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newUser.Email))
                throw new ArgumentException("Email is required", nameof(newUser));

            var existingUser = await GetUserByEmailAsync(newUser.Email, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogInformation("User with email {Email} already exists, returning existing user", newUser.Email);
                return existingUser;
            }

            _logger.LogInformation("Creating new user with email {Email}", newUser.Email);
        
            await AssignUserRolesAsync(newUser, cancellationToken);
            await HandleStaffMemberCreationAsync(newUser, cancellationToken);
            await ProcessUnclaimedAchievementsAsync(newUser, cancellationToken);

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _cacheService.Remove(CacheTags.NewlyUserCreated);
        
            _logger.LogInformation("Successfully created user with ID {UserId}", newUser.Id);
            return newUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email {Email}", newUser.Email);
            throw;
        }
    }

    private async Task AssignUserRolesAsync(User newUser, CancellationToken cancellationToken)
    {
        var requiredRoles = new List<string> { "User" };
        
        if (IsStaffEmail(newUser.Email!))
        {
            requiredRoles.Add("Staff");
        }

        var roles = await _dbContext.Roles
            .TagWithContext("GetRequiredRoles")
            .Where(r => requiredRoles.Contains(r.Name))
            .ToListAsync(cancellationToken);

        if (!requiredRoles.All(x => roles.Any(r => x == r.Name)))
        {
            var missingRoles = requiredRoles.Except(roles.Select(r => r.Name));
            throw new InvalidOperationException($"Missing required roles: {string.Join(", ", missingRoles)}");
        }

        foreach (var role in roles)
        {
            newUser.Roles.Add(new UserRole { Role = role });
        }
    }

    private async Task HandleStaffMemberCreationAsync(User newUser, CancellationToken cancellationToken)
    {
        if (!IsStaffEmail(newUser.Email!))
        {
            newUser.GenerateAchievement();
            return;
        }

        var existingStaff = await _dbContext.StaffMembers
            .TagWithContext("GetExistingStaff")
            .FirstOrDefaultAsync(s => s.Email == newUser.Email, cancellationToken);

        if (existingStaff == null)
        {
            var staffMember = new StaffMember 
            { 
                Email = newUser.Email, 
                Name = newUser.FullName ?? string.Empty 
            };

            staffMember.StaffAchievement = new Achievement
            {
                Name = staffMember.Name,
                Code = AchievementHelper.GenerateCode(),
                Type = AchievementType.Scanned,
                Value = 0
            };

            await _dbContext.StaffMembers.AddAsync(staffMember, cancellationToken);
        }
    }

    private async Task ProcessUnclaimedAchievementsAsync(User newUser, CancellationToken cancellationToken)
    {
        var unclaimedAchievements = await _dbContext.UnclaimedAchievements
            .TagWithContext("GetUnclaimedAchievements")
            .Include(ua => ua.Achievement)
            .Where(ua => ua.EmailAddress == newUser.Email)
            .ToListAsync(cancellationToken);

        if (unclaimedAchievements.Count == 0) return;

        var currentTime = DateTime.UtcNow;
        
        foreach (var unclaimedAchievement in unclaimedAchievements)
        {
            newUser.UserAchievements.Add(new UserAchievement
            {
                Achievement = unclaimedAchievement.Achievement,
                AwardedAt = currentTime,
                CreatedUtc = currentTime
            });

            _dbContext.UnclaimedAchievements.Remove(unclaimedAchievement);
        }
    }

    private bool IsStaffEmail(string email)
    {
        return new MailAddress(email).Host == _staffSmtpDomain;
    }

    public async Task<int> GetUserId(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email cannot be null or empty");

        var userId = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (userId == 0)
            throw new NotFoundException($"No user found with email: {email}");

        return userId;

    }

    public CurrentUserDto GetCurrentUser()
    {
        return GetCurrentUser(CancellationToken.None).Result;
    }

    public async Task<CurrentUserDto> GetCurrentUser(CancellationToken cancellationToken)
    {
        var currentUserEmail = _currentUserService.GetUserEmail();

        if (string.IsNullOrWhiteSpace(currentUserEmail))
            throw new UnauthorizedAccessException("User is not logged in");

        var user = await GetOrCreateUserAsync(currentUserEmail, cancellationToken);

        await ActivateUserIfNotActive(user, cancellationToken);

        var currentUserDto = _mapper.Map<CurrentUserDto>(user);
        currentUserDto.Rank = await GetUserRankAsync(user.Id, cancellationToken);
        
        return currentUserDto;
    }

    private async Task<User> GetOrCreateUserAsync(string email, CancellationToken cancellationToken)
    {
        var user = await GetUserByEmailAsync(email, cancellationToken);

        if (user != null)
            return user;

        // Create a new user if they don't exist
        var newUser = new User
        {
            Email = email,
            FullName = _currentUserService.GetUserFullName(),
            Avatar = _currentUserService.GetUserProfilePic(),
            CreatedUtc = DateTime.UtcNow
        };

        return await CreateUser(newUser, cancellationToken);
    }

    private async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .TagWithContext("GetUserByEmail")
            .Where(u => u.Email == email)
            .Include(u => u.Achievement)
            .Include(u => u.UserAchievements)
                .ThenInclude(ua => ua.Achievement)
            .Include(u => u.UserRewards)
                .ThenInclude(ur => ur.Reward)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<int> GetUserRankAsync(int userId, CancellationToken cancellationToken)
    {
        var leaderboard = await _leaderboardService.GetFullLeaderboard(cancellationToken);
        var leaderboardRanked = leaderboard.OrderByDescending(x => x.TotalPoints);
        var userRank = leaderboardRanked.FirstOrDefault(u => u.UserId == userId);
    
        return userRank?.Rank ?? 0;
    }

    public async Task<int> GetCurrentUserId(CancellationToken cancellationToken)
    {
        string currentUserEmail = _currentUserService.GetUserEmail();

        var user = await _dbContext.Users
            .AsNoTracking()
            .TagWithContext()
            .Where(u => u.Email == currentUserEmail)
            .Select(x => new { x.Id, x.Activated })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(User), currentUserEmail);

        if (!user.Activated)
        {
            // Only update user active state in DB when needed. This should be rare.
            await ActivateUser(user.Id, cancellationToken);
        }

        return user.Id;
    }

    public IEnumerable<Role> GetCurrentUserRoles()
    {
        return GetCurrentUserRoles(CancellationToken.None).Result;
    }

    public async Task<IEnumerable<Role>> GetCurrentUserRoles(CancellationToken cancellationToken)
    {
        string email = _currentUserService.GetUserEmail();
        return await _dbContext.UserRoles
            .AsNoTracking()
            .TagWithContext()
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
                .TagWithContext()
                .Include(s => s.StaffAchievement)
                .Where(s => s.Email == emailAddress && !s.IsDeleted)
                .Select(s => s.StaffAchievement!.Code)
                .SingleOrDefaultAsync(cancellationToken);

        return code ?? string.Empty;
    }

    public async Task<UserProfileDto> GetUser(int userId, CancellationToken cancellationToken)
    {
        var vm = await _dbContext.Users
            .TagWithContext("GetUserProfile")
            .Where(u => u.Id == userId)
            .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        vm.Rank = await GetUserRankAsync(userId, cancellationToken);
        vm.IsStaff = vm.Email?.EndsWith("@ssw.com.au") ?? false;
        vm.Balance = vm.Points - vm.Rewards.Sum(r => r.RewardCost);

        return vm;
    }

    public UsersViewModel GetUsers()
    {
        return GetUsers(CancellationToken.None).Result;
    }

    public async Task<UsersViewModel> GetUsers(CancellationToken cancellationToken)
    {
        var vm = await _dbContext.Users
            .TagWithContext()
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
            .TagWithContext()
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
        var userRewards = await _dbContext.UserRewards
            .TagWithContext()
            .Where(ur => ur.UserId == userId)
            .ProjectTo<UserRewardDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new UserRewardsViewModel
        {
            UserId = userId,
            UserRewards = userRewards
        };

    }
    
    public UserPendingRedemptionsViewModel GetUserPendingRedemptions(int userId)
    {
        return GetUserPendingRedemptions(userId, CancellationToken.None).Result;
    }
    
    public async Task<UserPendingRedemptionsViewModel> GetUserPendingRedemptions(int userId, CancellationToken cancellationToken)
    {
        var pendingRedemptions = await _dbContext.PendingRedemptions
            .TagWithContext()
            .Where(x => x.User.Id == userId && !x.Completed && !x.CancelledByAdmin && !x.CancelledByUser)
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
            .TagWithContext()
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
        var userToEdit = await _dbContext.Users
            .TagWithContext("GetUserToEdit")
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var roleToRemove = await _dbContext.UserRoles
            .TagWithContext("GetRolesToRemove")
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

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

    private async Task ActivateUser(int userId, CancellationToken cancellationToken)
    {
        User user = await _dbContext.Users
            .TagWithContext()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        await ActivateUserIfNotActive(user, cancellationToken);
    }

    private async Task ActivateUserIfNotActive(User user, CancellationToken cancellationToken)
    {
        if (!user.Activated)
        {
            user.Activated = true;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _cacheService.Remove(CacheTags.NewlyUserCreated);
        }
    }
}