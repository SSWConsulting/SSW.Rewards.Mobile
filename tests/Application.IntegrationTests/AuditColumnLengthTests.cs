using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;
using Testcontainers.MsSql;

namespace SSW.Rewards.Infrastructure.IntegrationTests;

/// <summary>
/// Regression tests for #1580: the audit columns (CreatedBy / LastModifiedBy / DeletedBy) must hold
/// any user id the API accepts. Runs against a real SQL Server (Testcontainers) with the real EF
/// migrations applied, so a column that is too narrow fails with "String or binary data would be truncated".
/// Requires Docker.
/// </summary>
[TestFixture]
[NonParallelizable]
public class AuditColumnLengthTests
{
    // 100 chars — well past the old nvarchar(40) limit, e.g. an email used as the token `sub`.
    private static readonly string LongUserId =
        "audit-length-repro-0123456789abcdef-0123456789abcdef-0123456789abcdef-0123456789@test.local".PadRight(100, 'x');

    private MsSqlContainer _sqlContainer = null!;
    private ServiceProvider _provider = null!;
    private TestCurrentUserService _currentUserService = null!;

    [OneTimeSetUp]
    public async Task StartSqlServerAndMigrate()
    {
        _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await _sqlContainer.StartAsync();

        // Use a dedicated database rather than the container's default `master`; MigrateAsync creates it.
        var connectionString = new SqlConnectionStringBuilder(_sqlContainer.GetConnectionString())
        {
            InitialCatalog = "SSW.Rewards.AuditColumnLengthTests"
        }.ConnectionString;

        _currentUserService = new TestCurrentUserService();

        var services = new ServiceCollection();
        services.AddSingleton<ICurrentUserService>(_currentUserService);
        services.AddSingleton<IDateTime, TestDateTime>();
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
        services.AddSingleton<AchievementIntegrationIdInterceptor>();
        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            options.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            options.AddInterceptors(
                provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
                provider.GetRequiredService<AchievementIntegrationIdInterceptor>());
        });

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task StopSqlServer()
    {
        if (_provider is not null)
        {
            await _provider.DisposeAsync();
        }

        if (_sqlContainer is not null)
        {
            await _sqlContainer.DisposeAsync();
        }
    }

    [Test]
    public async Task CreatedBy_ShouldPersistUserIdLongerThan40Characters()
    {
        LongUserId.Length.Should().BeGreaterThan(40);
        _currentUserService.UserId = LongUserId;

        int skillId;
        using (var scope = _provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var skill = new Skill { Name = "Audit length skill" };
            context.Skills.Add(skill);
            await context.SaveChangesAsync();
            skillId = skill.Id;
        }

        using (var scope = _provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var saved = await context.Skills.AsNoTracking().SingleAsync(s => s.Id == skillId);

            saved.CreatedBy.Should().Be(LongUserId);
        }
    }

    [Test]
    public async Task AuditableEntity_ShouldPersistLongUserIdOnCreateUpdateAndSoftDelete()
    {
        _currentUserService.UserId = LongUserId;

        int achievementId;
        using (var scope = _provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var achievement = new Achievement { Name = "Audit length achievement" };
            context.Achievements.Add(achievement);
            await context.SaveChangesAsync();
            achievementId = achievement.Id;
        }

        using (var scope = _provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var achievement = await context.Achievements.SingleAsync(a => a.Id == achievementId);
            achievement.Name = "Audit length achievement (updated)";
            await context.SaveChangesAsync();

            // The interceptor turns a delete into a soft delete that stamps DeletedBy.
            context.Achievements.Remove(achievement);
            await context.SaveChangesAsync();
        }

        using (var scope = _provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var saved = await context.Achievements
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleAsync(a => a.Id == achievementId);

            saved.CreatedBy.Should().Be(LongUserId);
            saved.LastModifiedBy.Should().Be(LongUserId);
            saved.DeletedBy.Should().Be(LongUserId);
            saved.DeletedUtc.Should().NotBeNull();
        }
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
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}
