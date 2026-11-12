using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Application.Rewards.BackgroundJobs;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Enums;

namespace SSW.Rewards.Application.UnitTests.Rewards.BackgroundJobs;

[TestFixture]
public class RewardClaimDeliveryJobTests
{
    private static readonly DateTime Now = new(2026, 11, 12, 5, 0, 0, DateTimeKind.Utc);

    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IRewardSender> _senderMock = null!;
    private RewardClaimDelivery _delivery = null!;
    private RewardClaimDeliveryJob _job = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _senderMock = new Mock<IRewardSender>();

        var user = new User { Id = 3, Email = "user@ssw.com.au", FullName = "Test User" };
        var reward = new Reward { Id = 7, Name = "Hoodie", RewardType = RewardType.Physical };
        var userReward = new UserReward { Id = 42, UserId = 3, User = user, RewardId = 7, Reward = reward };

        _delivery = new RewardClaimDelivery
        {
            Id = 11,
            UserRewardId = 42,
            UserReward = userReward,
            FreeformAddress = "1 Main St, Sydney NSW 2000",
            Status = RewardClaimDeliveryStatus.Pending
        };

        _contextMock.Setup(x => x.RewardClaimDeliveries)
            .Returns(new List<RewardClaimDelivery> { _delivery }.BuildMockDbSet().Object);
        _contextMock.Setup(x => x.UserRewards)
            .Returns(new List<UserReward> { userReward }.BuildMockDbSet().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dateTime = new Mock<IDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(Now);

        _job = new RewardClaimDeliveryJob(
            _contextMock.Object,
            _senderMock.Object,
            dateTime.Object,
            Mock.Of<ILogger<RewardClaimDeliveryJob>>());
    }

    [Test]
    public async Task DeliverAsync_WhenEmailRefused_ThrowsSoHangfireRetries_AndRecordStaysPending()
    {
        _senderMock.Setup(x => x.SendRewardAsync(It.IsAny<RewardClaimNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = () => _job.DeliverAsync(_delivery.Id, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _delivery.Status.Should().Be(RewardClaimDeliveryStatus.Pending);
        _delivery.Attempts.Should().Be(1);
        _delivery.LastAttemptUtc.Should().Be(Now);
        _delivery.LastError.Should().NotBeNullOrEmpty();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeliverAsync_WhenEmailThrows_RecordsErrorAndRethrows()
    {
        _senderMock.Setup(x => x.SendRewardAsync(It.IsAny<RewardClaimNotification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unreachable"));

        var act = () => _job.DeliverAsync(_delivery.Id, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("SMTP unreachable");
        _delivery.Status.Should().Be(RewardClaimDeliveryStatus.Pending);
        _delivery.LastError.Should().Be("SMTP unreachable");
    }

    [Test]
    public async Task DeliverAsync_WhenSent_MarksDelivered_AndSecondRunDoesNotResend()
    {
        _senderMock.Setup(x => x.SendRewardAsync(It.IsAny<RewardClaimNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _job.DeliverAsync(_delivery.Id, CancellationToken.None);
        await _job.DeliverAsync(_delivery.Id, CancellationToken.None);

        _delivery.Status.Should().Be(RewardClaimDeliveryStatus.Delivered);
        _delivery.DeliveredUtc.Should().Be(Now);
        _delivery.Attempts.Should().Be(1);
        _senderMock.Verify(x => x.SendRewardAsync(
            It.Is<RewardClaimNotification>(n =>
                n.ClaimId == 42 &&
                n.Address == "1 Main St, Sydney NSW 2000" &&
                n.UserEmail == "user@ssw.com.au" &&
                n.RewardName == "Hoodie" &&
                n.RewardType == RewardType.Physical),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeliverAsync_RetryAfterFailure_SendsAgainUsingPersistedAddress()
    {
        _senderMock.SetupSequence(x => x.SendRewardAsync(It.IsAny<RewardClaimNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        var first = () => _job.DeliverAsync(_delivery.Id, CancellationToken.None);
        await first.Should().ThrowAsync<InvalidOperationException>();

        await _job.DeliverAsync(_delivery.Id, CancellationToken.None);

        _delivery.Status.Should().Be(RewardClaimDeliveryStatus.Delivered);
        _delivery.Attempts.Should().Be(2);
        _delivery.LastError.Should().BeNull();
    }

    [Test]
    public async Task SweepJob_EnqueuesOnlyStalePendingDeliveries()
    {
        var deliveries = new List<RewardClaimDelivery>
        {
            new() { Id = 1, Status = RewardClaimDeliveryStatus.Pending, CreatedUtc = Now.AddHours(-2) },
            new() { Id = 2, Status = RewardClaimDeliveryStatus.Pending, CreatedUtc = Now.AddHours(-2), LastAttemptUtc = Now.AddMinutes(-1) },
            new() { Id = 3, Status = RewardClaimDeliveryStatus.Pending, CreatedUtc = Now.AddMinutes(-1) },
            new() { Id = 4, Status = RewardClaimDeliveryStatus.Delivered, CreatedUtc = Now.AddHours(-2) },
        };
        _contextMock.Setup(x => x.RewardClaimDeliveries).Returns(deliveries.BuildMockDbSet().Object);

        var jobClient = new Mock<IBackgroundJobClient>();
        var dateTime = new Mock<IDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(Now);

        var sweep = new RewardClaimDeliverySweepJob(
            _contextMock.Object,
            jobClient.Object,
            dateTime.Object,
            Mock.Of<ILogger<RewardClaimDeliverySweepJob>>());

        await sweep.EnqueueStaleDeliveriesAsync(CancellationToken.None);

        jobClient.Verify(x => x.Create(
            It.Is<Job>(j => j.Type == typeof(RewardClaimDeliveryJob) && (int)j.Args[0] == 1),
            It.IsAny<EnqueuedState>()), Times.Once);
        jobClient.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Once);
    }
}
