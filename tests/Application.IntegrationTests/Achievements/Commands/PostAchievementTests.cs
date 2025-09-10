using SSW.Rewards.Application.Achievements.Command.PostAchievement;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Achievements;
using NUnit.Framework;
using FluentAssertions;

namespace SSW.Rewards.Application.IntegrationTests.Achievements.Commands;

using static Testing;

public class PostAchievementTests : BaseTestFixture
{
    [Test]
    public async Task Handle_MultiscanDisabled_ShouldPreventDuplicateAchievements()
    {
        // Arrange
        var achievement = new Achievement
        {
            Code = "TEST001",
            Name = "Test Achievement",
            Value = 100,
            Type = AchievementType.Code,
            Icon = Icons.Code,
            IsMultiscanEnabled = false // Multiscan disabled
        };

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "Test User"
        };

        await AddAsync(achievement);
        await AddAsync(user);

        var command = new PostAchievementCommand { Code = "TEST001" };

        // Act - First claim should succeed
        var result1 = await SendAsync(command);

        // Act - Second claim should return duplicate
        var result2 = await SendAsync(command);

        // Assert
        result1.status.Should().Be(ClaimAchievementStatus.Claimed);
        result2.status.Should().Be(ClaimAchievementStatus.Duplicate);

        // Verify only one UserAchievement record exists
        var userAchievementCount = await CountAsync<UserAchievement>();
        userAchievementCount.Should().Be(1);
    }

    [Test]
    public async Task Handle_MultiscanEnabled_ShouldAllowDuplicateAchievements()
    {
        // Arrange
        var achievement = new Achievement
        {
            Code = "TEST002",
            Name = "Test Multiscan Achievement",
            Value = 50,
            Type = AchievementType.Code,
            Icon = Icons.Code,
            IsMultiscanEnabled = true // Multiscan enabled
        };

        var user = new User
        {
            Id = 2,
            Email = "test2@example.com",
            FullName = "Test User 2"
        };

        await AddAsync(achievement);
        await AddAsync(user);

        var command = new PostAchievementCommand { Code = "TEST002" };

        // Act - Both claims should succeed
        var result1 = await SendAsync(command);
        var result2 = await SendAsync(command);

        // Assert
        result1.status.Should().Be(ClaimAchievementStatus.Claimed);
        result2.status.Should().Be(ClaimAchievementStatus.Claimed);

        // Verify two UserAchievement records exist
        var userAchievementCount = await CountAsync<UserAchievement>();
        userAchievementCount.Should().Be(2);
    }

    [Test]
    public async Task Handle_RapidMultipleScans_MultiscanDisabled_ShouldPreventRaceCondition()
    {
        // Arrange
        var achievement = new Achievement
        {
            Code = "TEST003",
            Name = "Race Condition Test Achievement",
            Value = 200,
            Type = AchievementType.Code,
            Icon = Icons.Code,
            IsMultiscanEnabled = false // Multiscan disabled
        };

        var user = new User
        {
            Id = 3,
            Email = "test3@example.com",
            FullName = "Test User 3"
        };

        await AddAsync(achievement);
        await AddAsync(user);

        var command = new PostAchievementCommand { Code = "TEST003" };

        // Act - Simulate rapid concurrent scans
        var tasks = new List<Task<ClaimAchievementResult>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(SendAsync(command));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        // Only one should succeed, others should be duplicates
        var claimedCount = results.Count(r => r.status == ClaimAchievementStatus.Claimed);
        var duplicateCount = results.Count(r => r.status == ClaimAchievementStatus.Duplicate);

        claimedCount.Should().Be(1);
        duplicateCount.Should().Be(4);

        // Verify only one UserAchievement record exists
        var userAchievementCount = await CountAsync<UserAchievement>();
        userAchievementCount.Should().Be(1);
    }
}