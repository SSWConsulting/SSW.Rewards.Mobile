using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Users.Common;
using SSW.Rewards.Application.Users.Common.Interfaces;
using SSW.Rewards.Application.Users.Queries.GetCurrentUser;
using SSW.Rewards.Application.Users.Queries.GetUser;
using SSW.Rewards.Application.Users.Queries.GetUserRewards;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Services
{
    public class UserService : IUserService, IRolesService
    {
        private readonly ISSWRewardsDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly string StaffSMTPDomain;

        public UserService(ISSWRewardsDbContext dbContext, ICurrentUserService currentUserService, IMapper mapper, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _mapper = mapper;

            // TODO: @william update this to IOptionsPattern when upgrading to .NET 6
            StaffSMTPDomain = configuration.GetValue<string>(nameof(StaffSMTPDomain));
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

        public async Task<int> CreateUser(User user, CancellationToken cancellationToken)
        {
            var userRole = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == "User");

            user.Roles.Add(new UserRole { Role = userRole });

            var userEmail = new MailAddress(user.Email);

            if (userEmail.Host == StaffSMTPDomain)
            {
                var staffRole = await _dbContext.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Staff");

                user.Roles.Add(new UserRole { Role = staffRole });
            }

            _dbContext.Users.Add(user);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return user.Id;
        }

        public CurrentUserViewModel GetCurrentUser()
        {
            return GetCurrentUser(CancellationToken.None).Result;
        }

        public async Task<CurrentUserViewModel> GetCurrentUser(CancellationToken cancellationToken)
        {
            string currentUserEmail = _currentUserService.GetUserEmail();

            var user = await _dbContext.Users
                    .Where(u => u.Email == currentUserEmail)
                    .SingleOrDefaultAsync(cancellationToken);

            if (!user.Activated)
            {
                user.Activated = true;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return _mapper.Map<CurrentUserViewModel>(user);
        }

        public IEnumerable<Role> GetCurrentUserRoles()
        {
            return GetCurrentUserRoles(CancellationToken.None).Result;
        }

        public async Task<IEnumerable<Role>> GetCurrentUserRoles(CancellationToken cancellationToken)
        {
            var roles = new List<Role>();

            var userRoles = await _dbContext.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.Email == _currentUserService.GetUserEmail())
                .Select(u => u.Roles)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            foreach(var role in userRoles)
            {
                roles.Add(role.Role);
            }

            return roles;
        }

        public string GetStaffQRCode(string emailAddress)
        {
            return GetStaffQRCode(emailAddress, CancellationToken.None).Result;
        }

        public async Task<string> GetStaffQRCode(string emailAddress, CancellationToken cancellationToken)
        {
            var achievement = await _dbContext.StaffMembers
                    .Include(s => s.StaffAchievement)
                    .Where(s => s.Email == emailAddress)
                    .Select(s => s.StaffAchievement)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(cancellationToken);

            return achievement.Code;
        }

        public UserViewModel GetUser(int userId)
        {
            return GetUser(userId, CancellationToken.None).Result;
        }

        public async Task<UserViewModel> GetUser(int userId, CancellationToken cancellationToken)
        {
            var vm = await _dbContext.Users
                    .Where(u => u.Id == userId)
                    .ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
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

        public IEnumerable<Role> GetUserRoles(int userId)
        {
            return GetUserRoles(userId, CancellationToken.None).Result;
        }

        public async Task<IEnumerable<Role>> GetUserRoles(int userId, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.Where(u => u.Id == userId)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            var roles = new List<Role>();

            foreach (var role in user.Roles)
            {
                roles.Add(role.Role);
            }

            return roles;
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
}
