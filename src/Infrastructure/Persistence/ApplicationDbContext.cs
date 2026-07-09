using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    //private readonly IMediator _mediator;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
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
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<DeviceToken> DeviceTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizAnswer> QuizAnswers { get; set; }
    public DbSet<CompletedQuiz> CompletedQuizzes { get; set; }
    public DbSet<SocialMediaPlatform> SocialMediaPlatforms { get; set; }
    public DbSet<UserSocialMediaId> UserSocialMediaIds { get; set; }
    public DbSet<SubmittedQuizAnswer> SubmittedAnswers { get; set; }
    public DbSet<UnclaimedAchievement> UnclaimedAchievements { get; set; }
    public DbSet<OpenProfileDeletionRequest> OpenProfileDeletionRequests { get; set; }
    public DbSet<PendingRedemption> PendingRedemptions { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<PostComment> PostComments { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantSettings> TenantSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
