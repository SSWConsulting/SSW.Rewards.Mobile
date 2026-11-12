using AutoMapper;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Rewards.BackgroundJobs;
using SSW.Rewards.Application.Rewards.Commands.ClaimReward;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.AddressTypes;

namespace SSW.Rewards.Application.UnitTests.Rewards.Commands;

/// <summary>
/// ClaimRewardCommand saves the claim and its delivery outbox record together, then enqueues delivery.
/// The email is never sent in the request, so it can't turn a saved claim into an error (#1579).
/// </summary>
[TestFixture]
public class ClaimRewardCommandTests
{
    private const string UserEmail = "user@ssw.com.au";

    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IBackgroundJobClient> _jobClientMock = null!;
    private Mock<ICacheService> _cacheMock = null!;
    private ClaimRewardCommandHandler _handler = null!;

    private List<UserReward> _addedUserRewards = null!;
    private List<RewardClaimDelivery> _addedDeliveries = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _jobClientMock = new Mock<IBackgroundJobClient>();
        _cacheMock = new Mock<ICacheService>();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.GetUserEmail()).Returns(UserEmail);

        _cacheMock
            .Setup(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<Task<int>>>()))
            .ReturnsAsync(-1);

        var reward = new Reward { Id = 7, Code = "HOODIE", Name = "Hoodie", Cost = 100, RewardType = RewardType.Physical };
        _contextMock.Setup(x => x.Rewards).Returns(new List<Reward> { reward }.BuildMockDbSet().Object);

        var user = new User { Id = 3, Email = UserEmail, FullName = "Test User" };
        user.UserAchievements.Add(new UserAchievement { Achievement = new Achievement { Value = 500 } });
        _contextMock.Setup(x => x.Users).Returns(new List<User> { user }.BuildMockDbSet().Object);

        _addedUserRewards = [];
        var userRewards = new List<UserReward>().BuildMockDbSet();
        userRewards.Setup(x => x.Add(It.IsAny<UserReward>())).Callback<UserReward>(_addedUserRewards.Add);
        _contextMock.Setup(x => x.UserRewards).Returns(userRewards.Object);

        _addedDeliveries = [];
        var deliveries = new List<RewardClaimDelivery>().BuildMockDbSet();
        deliveries.Setup(x => x.Add(It.IsAny<RewardClaimDelivery>())).Callback<RewardClaimDelivery>(_addedDeliveries.Add);
        _contextMock.Setup(x => x.RewardClaimDeliveries).Returns(deliveries.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new ClaimRewardCommandHandler(
            _contextMock.Object,
            Mock.Of<IMapper>(),
            _jobClientMock.Object,
            currentUser.Object,
            _cacheMock.Object,
            Mock.Of<ILogger<ClaimRewardCommandHandler>>());
    }

    [Test]
    public async Task Handle_ShippedReward_SavesPendingDeliveryWithAddressInSameSaveAsClaim()
    {
        var command = new ClaimRewardCommand
        {
            Id = 7,
            ClaimInPerson = false,
            Address = new Address
            {
                streetNumber = "1",
                streetName = "Main St",
                municipality = "Sydney",
                countrySubdivisionName = "New South Wales",
                postalCode = "2000",
                country = "Australia",
                freeformAddress = "1 Main St, Sydney NSW 2000"
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.status.Should().Be(RewardStatus.Claimed);
        _addedUserRewards.Should().ContainSingle();
        var delivery = _addedDeliveries.Should().ContainSingle().Subject;
        delivery.UserReward.Should().BeSameAs(_addedUserRewards[0]);
        delivery.Status.Should().Be(RewardClaimDeliveryStatus.Pending);
        delivery.FreeformAddress.Should().Be("1 Main St, Sydney NSW 2000");
        delivery.StreetNumber.Should().Be("1");
        delivery.StreetName.Should().Be("Main St");
        delivery.Municipality.Should().Be("Sydney");
        delivery.State.Should().Be("New South Wales");
        delivery.PostalCode.Should().Be("2000");
        delivery.Country.Should().Be("Australia");

        // Claim and delivery record go to the database in one SaveChanges (one transaction).
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _jobClientMock.Verify(x => x.Create(
            It.Is<Job>(j => j.Type == typeof(RewardClaimDeliveryJob) && j.Method.Name == nameof(RewardClaimDeliveryJob.DeliverAsync)),
            It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenEnqueueFails_StillReturnsClaimedAndKeepsPendingRecord()
    {
        _jobClientMock
            .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Throws(new InvalidOperationException("Hangfire storage unavailable"));

        var result = await _handler.Handle(new ClaimRewardCommand { Id = 7, ClaimInPerson = false }, CancellationToken.None);

        result.status.Should().Be(RewardStatus.Claimed);
        _addedDeliveries.Should().ContainSingle().Which.Status.Should().Be(RewardClaimDeliveryStatus.Pending);
    }

    [Test]
    public async Task Handle_InPersonClaim_CreatesNoDeliveryAndEnqueuesNothing()
    {
        var result = await _handler.Handle(new ClaimRewardCommand { Id = 7, ClaimInPerson = true }, CancellationToken.None);

        result.status.Should().Be(RewardStatus.Claimed);
        _addedUserRewards.Should().ContainSingle();
        _addedDeliveries.Should().BeEmpty();
        _jobClientMock.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
    }
}
