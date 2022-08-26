using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;

namespace SSW.Rewards.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    //private readonly IMediator _mediator;
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        //IMediator mediator,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor
        ) 
        : base(options)
    {
        //_mediator = mediator;
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
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
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizAnswer> QuizAnswers { get; set; }
    public DbSet<CompletedQuiz> CompletedQuizzes { get; set; }
    public DbSet<SocialMediaPlatform> SocialMediaPlatforms { get; set; }
    public DbSet<UserSocialMediaId> UserSocialMediaIds { get; set; }
    public DbSet<SubmittedQuizAnswer> SubmittedAnswers { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }

    //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    //{
    //    await _mediator.DispatchDomainEvents(this);

    //    return await base.SaveChangesAsync(cancellationToken);
    //}
}
