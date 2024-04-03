namespace SSW.Rewards.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<StaffMember> StaffMembers { get; set; }
    DbSet<StaffMemberSkill> StaffMemberSkills { get; set; }
    DbSet<Skill> Skills { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<UserAchievement> UserAchievements { get; set; }
    DbSet<Achievement> Achievements { get; set; }
    DbSet<UserReward> UserRewards { get; set; }
    DbSet<Reward> Rewards { get; set; }
    DbSet<PostalAddress> Addresses { get; set; }
    DbSet<Notification> Notifications { get; set; }
    DbSet<Role> Roles { get; set; }
    DbSet<UserRole> UserRoles { get; set; }
    DbSet<Quiz> Quizzes { get; set; }
    DbSet<QuizQuestion> QuizQuestions { get; set; }
    DbSet<CompletedQuiz> CompletedQuizzes { get; set; }
    DbSet<SocialMediaPlatform> SocialMediaPlatforms { get; set; }
    DbSet<UserSocialMediaId> UserSocialMediaIds { get; set; }
    DbSet<QuizAnswer> QuizAnswers { get; set; }
    DbSet<SubmittedQuizAnswer> SubmittedAnswers { get; set; }
    DbSet<UnclaimedAchievement> UnclaimedAchievements { get; set; }
    DbSet<OpenProfileDeletionRequest> OpenProfileDeletionRequests { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
