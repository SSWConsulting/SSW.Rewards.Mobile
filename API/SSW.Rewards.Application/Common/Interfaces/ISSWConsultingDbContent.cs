using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Common.Interfaces
{
    public interface ISSWRewardsDbContext
    {
        DbSet<StaffMember> StaffMembers { get; set; }
        DbSet<StaffMemberSkill> StaffMemberSkills { get; set; }
        DbSet<Skill> Skills { get; set; }
        DbSet<Domain.Entities.User> Users { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<Domain.Entities.Achievement> Achievements { get; set; }
        public DbSet<UserReward> UserRewards { get; set; }
        public DbSet<Domain.Entities.Reward> Rewards { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
