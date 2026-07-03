using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.System.Commands.Common;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;

namespace SSW.Rewards.Application.UnitTests.System;

/// <summary>
/// Tests for the Northwind demo data seeder — idempotency, gap-fill on re-runs,
/// the no-negative-balance invariant and dev-user creation. Uses the EF InMemory
/// provider with the real ApplicationDbContext (a fresh store per test, a fresh
/// context per seed run to mimic separate CLI invocations).
/// </summary>
[TestFixture]
public class DemoDataSeederTests
{
    private static readonly DateTime Today = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string DevEmail = "dev@example.com";

    private string _dbName = null!;

    [SetUp]
    public void Setup() => _dbName = Guid.NewGuid().ToString();

    private ApplicationDbContext CreateContext()
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.GetUserId()).Returns("test-runner");
        var dateTime = new Mock<IDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(Today);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
        return new ApplicationDbContext(
            options,
            new AuditableEntitySaveChangesInterceptor(currentUser.Object, dateTime.Object),
            new AchievementIntegrationIdInterceptor());
    }

    private async Task<DemoSeedSummary> Seed(DateTime today, int years = 1)
    {
        await using var context = CreateContext();
        var seeder = new DemoDataSeeder(context);
        return await seeder.SeedAsync(
            new DemoSeedOptions { DevEmail = DevEmail, Years = years, Today = today },
            CancellationToken.None);
    }

    [Test]
    public async Task Seed_RunTwice_IsIdempotent()
    {
        var first = await Seed(Today);
        var second = await Seed(Today);

        second.AwardsAdded.Should().Be(0, "same-day re-runs must not duplicate history");
        second.ClaimsAdded.Should().Be(0);
        second.CompletionsAdded.Should().Be(0);
        second.PendingAdded.Should().Be(0);
        first.AwardsAdded.Should().BeGreaterThan(0);

        await using var context = CreateContext();
        (await context.Users.CountAsync()).Should().Be(first.Users);
    }

    [Test]
    public async Task Seed_ReRunAfterAGap_TopsUpNewDaysOnly()
    {
        await Seed(Today);
        var topUp = await Seed(Today.AddDays(60));

        topUp.AwardsAdded.Should().BeGreaterThan(0, "two months of new activity should be generated");

        var again = await Seed(Today.AddDays(60));
        again.AwardsAdded.Should().Be(0, "the topped-up window must itself be idempotent");
    }

    [Test]
    public async Task Seed_NoUserEverHasANegativeBalance()
    {
        await Seed(Today, years: 3);

        await using var context = CreateContext();
        var balances = await context.Users
            .Select(u => new
            {
                u.Email,
                Balance = (u.UserAchievements.Sum(ua => (int?)ua.Achievement.Value) ?? 0)
                          - (u.UserRewards.Sum(ur => (int?)ur.Reward.Cost) ?? 0)
                          - (u.PendingRedemptions
                              .Where(pr => !pr.Completed && !pr.CancelledByUser && !pr.CancelledByAdmin)
                              .Sum(pr => (int?)pr.Reward.Cost) ?? 0),
            })
            .ToListAsync();

        balances.Should().OnlyContain(b => b.Balance >= 0);
    }

    [Test]
    public async Task Seed_CreatesTheDevUser_ReadyToBindToARealLogin()
    {
        await Seed(Today);

        await using var context = CreateContext();
        var dev = await context.Users
            .Include(u => u.Achievement)
            .Include(u => u.UserAchievements)
            .SingleAsync(u => u.Email == DevEmail);

        dev.Activated.Should().BeTrue("the leaderboard only shows activated users");
        dev.FullName.Should().NotBeNullOrEmpty();
        dev.Achievement.Should().NotBeNull("the dev needs their own scannable QR achievement");
        dev.UserAchievements.Should().NotBeEmpty("the dev gets history so their profile isn't a desert");
    }

    [Test]
    public async Task Seed_FlagshipUser_IsNearTheTopOfTheAllTimeLeaderboard()
    {
        await Seed(Today, years: 3);

        await using var context = CreateContext();
        var ranked = await context.Users
            .Select(u => new { u.FullName, Points = u.UserAchievements.Sum(ua => (int?)ua.Achievement.Value) ?? 0 })
            .OrderByDescending(u => u.Points)
            .Take(3)
            .ToListAsync();

        ranked.Should().Contain(u => u.FullName == DemoDataSet.Flagship.Name,
            "the boss attends everything — he belongs at the top of the demo leaderboard");
    }

    [Test]
    public async Task Seed_PlayableQuizzesHaveNoCompletions()
    {
        await Seed(Today);

        await using var context = CreateContext();
        var playable = await context.Quizzes
            .Where(q => !q.IsArchived)
            .Select(q => new { q.Title, Completions = q.CompletedQuizzes.Count })
            .ToListAsync();

        playable.Should().NotBeEmpty();
        // Any passed completion blocks ALL further submissions of a quiz
        // (SubmitUserQuizCommandValidator checks globally, not per-user).
        playable.Should().OnlyContain(q => q.Completions == 0);
    }
}
