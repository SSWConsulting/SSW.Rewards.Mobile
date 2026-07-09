using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;

namespace SSW.Rewards.Infrastructure.IntegrationTests;

public class DbContextPoolingTests
{
    [Test]
    public async Task PooledContextShouldUseCurrentUserForEachScope()
    {
        var (provider, currentUserService) = BuildServiceProvider();
        using (provider)
        {
            currentUserService.UserId = "user-1";
            var firstSkillId = await AddSkillAsync(provider, "Pooling test 1");

            currentUserService.UserId = "user-2";
            var secondSkillId = await AddSkillAsync(provider, "Pooling test 2");

            using var assertionScope = provider.CreateScope();
            var context = assertionScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var firstSavedSkill = await context.Skills.SingleAsync(s => s.Id == firstSkillId);
            var secondSavedSkill = await context.Skills.SingleAsync(s => s.Id == secondSkillId);

            firstSavedSkill.CreatedBy.Should().Be("user-1");
            secondSavedSkill.CreatedBy.Should().Be("user-2");
        }
    }

    [Test]
    public async Task PooledContextShouldRunAchievementIntegrationIdInterceptor()
    {
        var (provider, _) = BuildServiceProvider();
        using (provider)
        {
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var firstAchievement = new Achievement { Name = "Pooling achievement 1", IntegrationId = "duplicate" };
            var secondAchievement = new Achievement { Name = "Pooling achievement 2", IntegrationId = "duplicate" };

            context.Achievements.AddRange(firstAchievement, secondAchievement);
            await context.SaveChangesAsync();

            firstAchievement.IntegrationId.Should().NotBeNullOrWhiteSpace();
            secondAchievement.IntegrationId.Should().NotBeNullOrWhiteSpace();
            firstAchievement.IntegrationId.Should().NotBe(secondAchievement.IntegrationId);
        }
    }

    private static async Task<int> AddSkillAsync(IServiceProvider provider, string name)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var skill = new Skill { Name = name };

        context.Skills.Add(skill);
        await context.SaveChangesAsync();

        return skill.Id;
    }

    private static (ServiceProvider Provider, TestCurrentUserService CurrentUserService) BuildServiceProvider()
    {
        var currentUserService = new TestCurrentUserService();
        var services = new ServiceCollection();

        services.AddSingleton<ICurrentUserService>(currentUserService);
        services.AddSingleton<IDateTime>(new TestDateTime());
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
        services.AddSingleton<AchievementIntegrationIdInterceptor>();
        services.AddDbContextPool<ApplicationDbContext>((provider, options) =>
        {
            options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            options.AddInterceptors(
                provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
                provider.GetRequiredService<AchievementIntegrationIdInterceptor>());
        });

        return (services.BuildServiceProvider(), currentUserService);
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string UserId { get; set; } = string.Empty;

        public string GetUserId() => UserId;

        public string GetUserEmail() => string.Empty;

        public string GetUserFullName() => string.Empty;

        public string? GetUserProfilePic() => null;

        public bool IsInRole(string role) => false;
    }

    private sealed class TestDateTime : IDateTime
    {
        public DateTime Now => new(2026, 7, 9, 0, 0, 0, DateTimeKind.Local);

        public DateTime UtcNow => new(2026, 7, 9, 0, 0, 0, DateTimeKind.Utc);
    }
}
