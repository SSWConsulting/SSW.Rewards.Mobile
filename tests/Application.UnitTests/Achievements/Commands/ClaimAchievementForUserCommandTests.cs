using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Achievements.Command.ClaimAchievementForUser;
using SSW.Rewards.Application.Achievements.Notifications;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Enums;

namespace SSW.Rewards.Application.UnitTests.Achievements.Commands;

/// <summary>
/// Unit tests for ClaimAchievementForUserCommand - handles achievement claiming for users.
/// Tests achievement validation, duplicate detection, user validation, and milestone triggers.
/// </summary>
[TestFixture]
public class ClaimAchievementForUserCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ILogger<ClaimAchievementForUserCommand>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;
    private ClaimAchievementForUserCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<ClaimAchievementForUserCommand>>();
        _mediatorMock = new Mock<IMediator>();

        _handler = new ClaimAchievementForUserCommandHandler(
            _contextMock.Object,
            _loggerMock.Object,
            _mediatorMock.Object);
    }

    [Test]
    public async Task Handle_WithValidAchievement_ShouldClaimSuccessfully()
    {
        // Arrange
        const int userId = 1;
        const int achievementId = 10;
        const string achievementCode = "TEST_ACHIEVEMENT";

        SetupAchievementQuery(achievementId, achievementCode);
        SetupUserQuery(userId);

        var capturedAchievements = new List<UserAchievement>();
        var mockUserAchievements = new List<UserAchievement>().BuildMockDbSet();
        mockUserAchievements.Setup(x => x.Add(It.IsAny<UserAchievement>()))
            .Callback<UserAchievement>(capturedAchievements.Add);

        _contextMock.Setup(x => x.UserAchievements).Returns(mockUserAchievements.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new ClaimAchievementForUserCommand
        {
            UserId = userId,
            Code = achievementCode
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.status.Should().Be(ClaimAchievementStatus.Claimed);

        capturedAchievements.Should().HaveCount(1);
        capturedAchievements[0].UserId.Should().Be(userId);
        capturedAchievements[0].AchievementId.Should().Be(achievementId);

        _mediatorMock.Verify(x => x.Publish(
            It.Is<UserMilestoneAchievementCheckRequested>(n => n.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentAchievement_ShouldReturnNotFound()
    {
        // Arrange
        const int userId = 1;
        const string nonExistentCode = "INVALID_CODE";

        SetupAchievementQuery(null, nonExistentCode);

        var command = new ClaimAchievementForUserCommand
        {
            UserId = userId,
            Code = nonExistentCode
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.status.Should().Be(ClaimAchievementStatus.NotFound);

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithNonExistentUser_ShouldReturnError()
    {
        // Arrange
        const int nonExistentUserId = 999;
        const string achievementCode = "TEST_ACHIEVEMENT";

        SetupAchievementQuery(10, achievementCode);
        SetupUserQuery(null);

        var command = new ClaimAchievementForUserCommand
        {
            UserId = nonExistentUserId,
            Code = achievementCode
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.status.Should().Be(ClaimAchievementStatus.Error);

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithDuplicateAchievement_ShouldReturnDuplicate()
    {
        // Arrange
        const int userId = 1;
        const int achievementId = 10;
        const string achievementCode = "TEST_ACHIEVEMENT";

        var existingAchievement = new UserAchievement
        {
            UserId = userId,
            AchievementId = achievementId,
            Achievement = new Achievement { Code = achievementCode }
        };

        SetupAchievementQuery(achievementId, achievementCode);
        SetupUserQuery(userId);
        SetupUserAchievementsQuery(new List<UserAchievement> { existingAchievement });

        var command = new ClaimAchievementForUserCommand
        {
            UserId = userId,
            Code = achievementCode
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.status.Should().Be(ClaimAchievementStatus.Duplicate);

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenSaveFails_ShouldReturnError()
    {
        // Arrange
        const int userId = 1;
        const int achievementId = 10;
        const string achievementCode = "TEST_ACHIEVEMENT";

        SetupAchievementQuery(achievementId, achievementCode);
        SetupUserQuery(userId);

        var mockUserAchievements = new List<UserAchievement>().BuildMockDbSet();
        _contextMock.Setup(x => x.UserAchievements).Returns(mockUserAchievements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new ClaimAchievementForUserCommand
        {
            UserId = userId,
            Code = achievementCode
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.status.Should().Be(ClaimAchievementStatus.Error);

        _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_OnSuccess_ShouldPublishMilestoneCheckNotification()
    {
        // Arrange
        const int userId = 5;
        const int achievementId = 15;
        const string achievementCode = "MILESTONE_TRIGGER";

        SetupAchievementQuery(achievementId, achievementCode);
        SetupUserQuery(userId);

        var mockUserAchievements = new List<UserAchievement>().BuildMockDbSet();
        _contextMock.Setup(x => x.UserAchievements).Returns(mockUserAchievements.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        UserMilestoneAchievementCheckRequested? capturedNotification = null;
        _mediatorMock.Setup(x => x.Publish(It.IsAny<UserMilestoneAchievementCheckRequested>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, ct) => capturedNotification = n as UserMilestoneAchievementCheckRequested)
            .Returns(Task.CompletedTask);

        var command = new ClaimAchievementForUserCommand
        {
            UserId = userId,
            Code = achievementCode
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.UserId.Should().Be(userId);
    }

    [Test]
    public async Task Handle_WithMultipleDifferentUsers_ShouldClaimForEach()
    {
        // Arrange
        const string achievementCode = "SHARED_ACHIEVEMENT";
        const int achievementId = 20;

        var capturedAchievements = new List<UserAchievement>();
        var mockUserAchievements = new List<UserAchievement>().BuildMockDbSet();
        mockUserAchievements.Setup(x => x.Add(It.IsAny<UserAchievement>()))
            .Callback<UserAchievement>(capturedAchievements.Add);

        _contextMock.Setup(x => x.UserAchievements).Returns(mockUserAchievements.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // User 1
        SetupAchievementQuery(achievementId, achievementCode);
        SetupUserQuery(1);

        var command1 = new ClaimAchievementForUserCommand { UserId = 1, Code = achievementCode };
        await _handler.Handle(command1, CancellationToken.None);

        // User 2
        SetupAchievementQuery(achievementId, achievementCode);
        SetupUserQuery(2);

        var command2 = new ClaimAchievementForUserCommand { UserId = 2, Code = achievementCode };
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        capturedAchievements.Should().HaveCount(2);
        capturedAchievements[0].UserId.Should().Be(1);
        capturedAchievements[1].UserId.Should().Be(2);
    }

    // Helper methods
    private void SetupAchievementQuery(int? achievementId, string code)
    {
        var achievements = achievementId.HasValue
            ? new List<Achievement> { new() { Id = achievementId.Value, Code = code } }
            : new List<Achievement>();

        var mockDbSet = achievements.BuildMockDbSet();
        _contextMock.Setup(x => x.Achievements).Returns(mockDbSet.Object);
    }

    private void SetupUserQuery(int? userId)
    {
        var users = userId.HasValue
            ? new List<User> { new() { Id = userId.Value } }
            : new List<User>();

        var mockDbSet = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockDbSet.Object);
    }

    private void SetupUserAchievementsQuery(List<UserAchievement> userAchievements)
    {
        var mockDbSet = userAchievements.BuildMockDbSet();
        _contextMock.Setup(x => x.UserAchievements).Returns(mockDbSet.Object);
    }
}

