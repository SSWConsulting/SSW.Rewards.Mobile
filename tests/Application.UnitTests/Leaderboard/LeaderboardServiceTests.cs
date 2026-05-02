using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Constants;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Leaderboard;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Application.UnitTests.Leaderboard;

/// <summary>
/// Unit tests for LeaderboardService - handles leaderboard data generation and retrieval.
/// Tests point calculations, user filtering, and caching behavior.
/// </summary>
[TestFixture]
public class LeaderboardServiceTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IDateTime> _dateTimeMock = null!;
    private Mock<ICacheService> _cacheServiceMock = null!;
    private Mock<IProfilePicStorageProvider> _profilePicStorageProviderMock = null!;
    private LeaderboardService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _dateTimeMock = new Mock<IDateTime>();
        _cacheServiceMock = new Mock<ICacheService>();
        _profilePicStorageProviderMock = new Mock<IProfilePicStorageProvider>();

        _dateTimeMock.Setup(x => x.UtcNow).Returns(new DateTime(2025, 12, 5, 12, 0, 0, DateTimeKind.Utc));
        _profilePicStorageProviderMock.Setup(x => x.GetProfilePicUri(It.IsAny<string>()))
            .ReturnsAsync(new Uri("https://example.com/default.jpg"));

        _service = new LeaderboardService(
            _contextMock.Object,
            _dateTimeMock.Object,
            _cacheServiceMock.Object,
            _profilePicStorageProviderMock.Object);
    }

    [Test]
    public async Task GetFullLeaderboard_WhenCached_ShouldReturnCachedData()
    {
        // Arrange
        var cachedLeaderboard = new List<LeaderboardUserDto>
        {
            new() { UserId = 1, Name = "User 1", TotalPoints = 100 },
            new() { UserId = 2, Name = "User 2", TotalPoints = 200 }
        };

        _cacheServiceMock.Setup(x => x.GetOrAddAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<LeaderboardUserDto>>>>()))
            .ReturnsAsync(cachedLeaderboard);

        // Act
        var result = await _service.GetFullLeaderboard(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(cachedLeaderboard);
    }

    [Test]
    public async Task GetUserById_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        const int targetUserId = 2;
        var leaderboard = new List<LeaderboardUserDto>
        {
            new() { UserId = 1, Name = "User 1", TotalPoints = 100 },
            new() { UserId = 2, Name = "User 2", TotalPoints = 200 },
            new() { UserId = 3, Name = "User 3", TotalPoints = 150 }
        };

        _cacheServiceMock.Setup(x => x.GetOrAddAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<LeaderboardUserDto>>>>()))
            .ReturnsAsync(leaderboard);

        // Act
        var result = await _service.GetUserById(targetUserId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(targetUserId);
        result.Name.Should().Be("User 2");
        result.TotalPoints.Should().Be(200);
    }

    [Test]
    public async Task GetUserById_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        const int nonExistentUserId = 999;
        var leaderboard = new List<LeaderboardUserDto>
        {
            new() { UserId = 1, Name = "User 1", TotalPoints = 100 },
            new() { UserId = 2, Name = "User 2", TotalPoints = 200 }
        };

        _cacheServiceMock.Setup(x => x.GetOrAddAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<LeaderboardUserDto>>>>()))
            .ReturnsAsync(leaderboard);

        // Act
        var result = await _service.GetUserById(nonExistentUserId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetFullLeaderboard_WithEmptyLeaderboard_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyLeaderboard = new List<LeaderboardUserDto>();

        _cacheServiceMock.Setup(x => x.GetOrAddAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<LeaderboardUserDto>>>>()))
            .ReturnsAsync(emptyLeaderboard);

        // Act
        var result = await _service.GetFullLeaderboard(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetFullLeaderboard_ShouldUseCacheKey()
    {
        // Arrange
        var leaderboard = new List<LeaderboardUserDto>();
        string? capturedCacheKey = null;

        _cacheServiceMock.Setup(x => x.GetOrAddAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<LeaderboardUserDto>>>>()))
            .Callback<string, Func<Task<List<LeaderboardUserDto>>>>((key, func) => capturedCacheKey = key)
            .ReturnsAsync(leaderboard);

        // Act
        await _service.GetFullLeaderboard(CancellationToken.None);

        // Assert
        capturedCacheKey.Should().NotBeNullOrEmpty();
        _cacheServiceMock.Verify(x => x.GetOrAddAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<List<LeaderboardUserDto>>>>()), Times.Once);
    }

    [Test]
    public async Task GetFullLeaderboard_WhenGenerating_ShouldCountThisMonthAsLastThirtyDaysAndKeepThisYear()
    {
        // Arrange
        var utcNow = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        _dateTimeMock.Setup(x => x.UtcNow).Returns(utcNow);

        var users = new List<User>
        {
            new()
            {
                Id = 1,
                FullName = "Monthly User",
                Email = "monthly@example.com",
                Activated = true,
                UserAchievements =
                [
                    new UserAchievement
                    {
                        AwardedAt = utcNow.AddDays(-18),
                        Achievement = new Achievement { Value = 10 }
                    },
                    new UserAchievement
                    {
                        AwardedAt = utcNow.AddDays(-1),
                        Achievement = new Achievement { Value = 20 }
                    },
                    new UserAchievement
                    {
                        AwardedAt = utcNow.AddDays(-33),
                        Achievement = new Achievement { Value = 30 }
                    }
                ]
            }
        };

        _contextMock.Setup(x => x.Users).Returns(users.BuildMockDbSet().Object);
        _cacheServiceMock.Setup(x => x.GetOrAddAsync(
                CacheKeys.Leaderboard,
                It.IsAny<Func<Task<List<LeaderboardUserDto>>>>()))
            .Returns<string, Func<Task<List<LeaderboardUserDto>>>>((_, factory) => factory());

        // Act
        var result = await _service.GetFullLeaderboard(CancellationToken.None);

        // Assert
        var user = result.Should().ContainSingle().Subject;
        user.PointsThisMonth.Should().Be(30);
        user.PointsThisYear.Should().Be(20);
    }
}
