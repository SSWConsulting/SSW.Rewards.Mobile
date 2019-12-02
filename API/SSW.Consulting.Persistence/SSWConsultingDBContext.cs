using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Interfaces;
using SSW.Consulting.Domain.Entities;

namespace SSW.Consulting.Persistence
{
    public class SSWConsultingDbContext : DbContext, ISSWConsultingDbContext
    {
		public interface ISecrets
		{
            string SqlConnectionString { get; }
		}

		private readonly ISecrets _secrets;

		public SSWConsultingDbContext(ISecrets secrets)
        {
			_secrets = secrets;
		}

        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<StaffMemberSkill> StaffMemberSkills { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserReward> UserRewards { get; set; }
        public DbSet<Reward> Rewards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_secrets.SqlConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SSWConsultingDbContext).Assembly);
        }

    }
}
