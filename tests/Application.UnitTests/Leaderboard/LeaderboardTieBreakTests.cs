using FluentAssertions;
using MediatR;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Constants;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Leaderboard;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardPaginatedList;
using SSW.Rewards.Application.Leaderboard.Queries.GetMobileLeaderboard;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Application.UnitTests.Leaderboard;

[TestFixture]
public class LeaderboardTieBreakTests
{
    private static readonly int[] ExpectedOrder = [1, 2, 3];

    private static IEnumerable<int[]> InputOrders()
    {
        yield return [1, 2, 3];
        yield return [3, 2, 1];
        yield return [2, 3, 1];
    }

    [TestCaseSource(nameof(InputOrders))]
    public async Task GetFullLeaderboard_WhenGenerating_ShouldBreakNameAndScoreTiesByUserId(int[] inputOrder)
    {
        var utcNow = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        var dateTime = new Mock<IDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(utcNow);
        var profilePics = new Mock<IProfilePicStorageProvider>();
        profilePics.Setup(x => x.GetProfilePicUri(It.IsAny<string>())).ReturnsAsync(new Uri("https://example.com/default.jpg"));

        var users = inputOrder.Select(id => new User
        {
            Id = id,
            FullName = "Same Name",
            Email = $"user{id}@example.com",
            Activated = true,
            UserAchievements =
            [
                new UserAchievement { AwardedAt = utcNow, Achievement = new Achievement { Value = 100 } }
            ]
        }).ToList();

        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Users).Returns(users.BuildMockDbSet().Object);
        var cache = new Mock<ICacheService>();
        cache.Setup(x => x.GetOrAddAsync(CacheKeys.Leaderboard, It.IsAny<Func<Task<List<LeaderboardUserDto>>>>()))
            .Returns<string, Func<Task<List<LeaderboardUserDto>>>>((_, factory) => factory());
        var service = new LeaderboardService(context.Object, dateTime.Object, cache.Object, profilePics.Object);

        var result = await service.GetFullLeaderboard(CancellationToken.None);

        result.Select(x => x.UserId).Should().Equal(ExpectedOrder);
        result.Select(x => x.Rank).Should().Equal(1, 2, 3);
    }

    [TestCaseSource(nameof(InputOrders))]
    public async Task PaginatedList_ShouldBreakNameAndScoreTiesByUserId(int[] inputOrder)
    {
        var handler = CreateHandler<GetLeaderboardPaginatedListQuery, LeaderboardViewModel>(
            "SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardPaginatedList.Handler",
            TiedLeaderboard(inputOrder));

        foreach (var period in Enum.GetValues<LeaderboardFilter>())
        {
            var result = await handler.Handle(
                new GetLeaderboardPaginatedListQuery { Skip = 0, Take = 10, CurrentPeriod = period },
                CancellationToken.None);

            result.Users.Select(x => x.UserId).Should().Equal(ExpectedOrder, $"period {period}");
        }
    }

    [TestCaseSource(nameof(InputOrders))]
    public async Task MobileLeaderboard_ShouldBreakNameAndScoreTiesByUserId(int[] inputOrder)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.GetUserEmail()).Returns("user3@example.com");
        var handler = CreateHandler<GetMobileLeaderboardQuery, MobileLeaderboardViewModel>(
            "SSW.Rewards.Application.Leaderboard.Queries.GetMobileLeaderboard.GetMobileLeaderboardQueryHandler",
            TiedLeaderboard(inputOrder),
            currentUser.Object);

        foreach (var period in Enum.GetValues<LeaderboardFilter>())
        {
            var result = await handler.Handle(
                new GetMobileLeaderboardQuery { Page = 0, PageSize = 10, CurrentPeriod = period },
                CancellationToken.None);

            result.Items.Select(x => x.UserId).Should().Equal(ExpectedOrder, $"period {period}");
            result.MyRank.Should().Be(3, $"period {period}");
        }
    }

    private static ILeaderboardService TiedLeaderboard(int[] inputOrder)
    {
        var service = new Mock<ILeaderboardService>();
        service.Setup(x => x.GetFullLeaderboard(It.IsAny<CancellationToken>()))
            .ReturnsAsync(inputOrder.Select(id => new LeaderboardUserDto
            {
                UserId = id,
                Name = "Same Name",
                Email = $"user{id}@example.com",
                Rank = 1,
                TotalPoints = 100,
                PointsThisWeek = 100,
                PointsThisMonth = 100,
                PointsThisYear = 100
            }).ToList());
        return service.Object;
    }

    private static IRequestHandler<TRequest, TResponse> CreateHandler<TRequest, TResponse>(string typeName, params object[] args)
        where TRequest : IRequest<TResponse>
    {
        var type = typeof(LeaderboardService).Assembly.GetType(typeName, throwOnError: true)!;
        return (IRequestHandler<TRequest, TResponse>)Activator.CreateInstance(type, args)!;
    }
}
