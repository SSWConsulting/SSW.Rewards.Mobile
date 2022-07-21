using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Persistence
{
    public class SSWRewardsDbContext : DbContext, ISSWRewardsDbContext
    {
		public interface ISecrets
		{
            string SqlConnectionString { get; }
        }

		private readonly ISecrets _secrets;
		private readonly IDateTimeProvider _dateTimeProvider;

		public SSWRewardsDbContext(ISecrets secrets, IDateTimeProvider dateTimeProvider)
		{
			_secrets = secrets;
			_dateTimeProvider = dateTimeProvider;
		}

        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<StaffMemberSkill> StaffMemberSkills { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserReward> UserRewards { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<PostalAddress> Addresses { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
        public DbSet<CompletedQuiz> CompletedQuizzes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_secrets.SqlConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SSWRewardsDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach(var entry in ChangeTracker.Entries<Entity>())
            {
                if(entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedUtc = _dateTimeProvider.Now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

    }
}
