namespace SSW.Rewards.Domain.Entities;
public class Achievement : BaseEntity
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public int Value { get; set; }

    public AchievementType Type { get; set; }

    public Icons Icon { get; set; }

    public bool IconIsBranded { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsMultiscanEnabled { get; set; }

    public string? IntegrationId { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = new HashSet<UserAchievement>();
    public ICollection<Quiz> Quizzes { get; set; } = new HashSet<Quiz>();
    public ICollection<SocialMediaPlatform> SocialMediaPlatforms { get; set; } = new HashSet<SocialMediaPlatform>();

    private static readonly Random rng = new Random();
    private static readonly object lockObj = new object();

    public Achievement()
    {
        GenerateIntegrationId();
    }

    /// <summary>
    /// Restricted usage - Should ONLY be used from the Achievment constructor or the save changes interceptor. Generates or re-generates the IntegrationId for this achievement.
    /// </summary>
    public void GenerateIntegrationId()
    {
        var characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var integrationId = new char[10];

        lock(lockObj)
        {
            for (int i = 0; i < integrationId.Length; i++)
            {
                integrationId[i] = characters[rng.Next(characters.Length)];
            }
        }

        IntegrationId = new string(integrationId);
    }
}
