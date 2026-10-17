using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.System.Commands.Common;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Infrastructure.Persistence.Interceptors;
using ApiClientConstants = SSW.Rewards.ApiClient.Constants;

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
            .AddInterceptors(
                new AuditableEntitySaveChangesInterceptor(currentUser.Object, dateTime.Object),
                new AchievementIntegrationIdInterceptor())
            .Options;
        return new ApplicationDbContext(options);
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

    private async Task<List<int>> Balances()
    {
        await using var context = CreateContext();
        return await context.Users
            .Select(u => (u.UserAchievements.Sum(ua => (int?)ua.Achievement.Value) ?? 0)
                         - (u.UserRewards.Sum(ur => (int?)ur.Reward.Cost) ?? 0)
                         - (u.PendingRedemptions
                             .Where(pr => !pr.Completed && !pr.CancelledByUser && !pr.CancelledByAdmin)
                             .Sum(pr => (int?)pr.Reward.Cost) ?? 0))
            .ToListAsync();
    }

    [Test]
    public async Task Seed_NoUserEverHasANegativeBalance()
    {
        await Seed(Today, years: 3);

        (await Balances()).Should().OnlyContain(b => b >= 0);
    }

    [Test]
    public async Task Seed_WideningTheHistoryWindow_ChargesEveryPersistedClaim()
    {
        await Seed(Today, years: 1);
        await Seed(Today, years: 3);

        (await Balances()).Should().OnlyContain(b => b >= 0);

        var again = await Seed(Today, years: 3);
        again.ClaimsAdded.Should().Be(0, "the widened window must itself be idempotent");
        again.PendingAdded.Should().Be(0);
    }

    [Test]
    public async Task Seed_WhenPersistedClaimsAlreadyOverspend_RefusesInsteadOfSucceeding()
    {
        await Seed(Today);
        await using (var context = CreateContext())
        {
            var dev = await context.Users.SingleAsync(u => u.Email == DevEmail);
            var reward = await context.Rewards.OrderByDescending(r => r.Cost).FirstAsync();
            for (var i = 0; i < 100; i++)
                context.UserRewards.Add(new UserReward { UserId = dev.Id, RewardId = reward.Id, AwardedAt = Today });
            await context.SaveChangesAsync();
        }

        var rerun = () => Seed(Today);

        (await rerun.Should().ThrowAsync<InvalidOperationException>()).WithMessage($"*{DevEmail}*");
    }

    [Test]
    public async Task Seed_QrCodes_DecodeToPrefixesTheMobileScannerAccepts()
    {
        await Seed(Today);

        await using var context = CreateContext();
        var achievementCodes = await context.Achievements.Select(a => a.Code).ToListAsync();
        var rewardCodes = await context.Rewards.Select(r => r.Code).ToListAsync();
        var pendingCodes = await context.PendingRedemptions.Select(pr => pr.Code).ToListAsync();

        pendingCodes.Should().NotBeEmpty();
        achievementCodes.Should().OnlyContain(c => DecodeLikeScanner(c).StartsWith(ApiClientConstants.RewardsQRCodeAchievementPrefix));
        rewardCodes.Should().OnlyContain(c => DecodeLikeScanner(c).StartsWith(ApiClientConstants.RewardsQRCodeRewardPrefix));
        pendingCodes.Should().OnlyContain(c => DecodeLikeScanner(c).StartsWith(ApiClientConstants.RewardsQRCodePendingRewardPrefix));
    }

    // Mirrors MobileUI ScannerService.DecodeQrCode + StringHelpers.Base64Decode (MAUI, not referenceable here).
    private static string DecodeLikeScanner(string? code)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(code ?? string.Empty)).ToLowerInvariant();
        }
        catch (FormatException)
        {
            return string.Empty;
        }
    }

    [Test]
    public async Task Seed_CancelledOrCompletedPendingRedemptions_AreNotRecreated()
    {
        await Seed(Today);
        await using (var context = CreateContext())
        {
            var pending = await context.PendingRedemptions.ToListAsync();
            pending.Should().HaveCountGreaterThan(1);
            pending[0].Completed = true;
            foreach (var pr in pending.Skip(1)) pr.CancelledByUser = true;
            await context.SaveChangesAsync();
        }

        var rerun = await Seed(Today);

        rerun.PendingAdded.Should().Be(0);
        await using var verify = CreateContext();
        var codes = await verify.PendingRedemptions.Select(pr => pr.Code).ToListAsync();
        codes.Should().OnlyHaveUniqueItems();
        (await verify.PendingRedemptions.CountAsync(pr => !pr.Completed && !pr.CancelledByUser)).Should().Be(0);
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
