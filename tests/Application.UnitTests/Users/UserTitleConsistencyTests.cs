using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.ActivityFeed.Queries;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Helpers;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Application.Leaderboard;
using SSW.Rewards.Application.Services;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Enums;
using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.Application.UnitTests.Users;

/// <summary>
/// #1581 - a user's title must be the same on their profile, in the Activity feed and on the leaderboard.
/// Rule: staff record title (matched by email, while it has a staff achievement) → Company link → "Community".
/// </summary>
[TestFixture]
public class UserTitleConsistencyTests
{
    private const string ViewerEmail = "viewer@example.com";

    private ApplicationDbContext _db = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private LeaderboardService _leaderboardService = null!;
    private UserService _userService = null!;
    private int _companyPlatformId;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        _currentUser = new Mock<ICurrentUserService>();
        _currentUser.Setup(x => x.GetUserEmail()).Returns(ViewerEmail);

        var dateTime = new Mock<IDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc));

        var cache = new Mock<ICacheService>();
        cache.Setup(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<Task<List<SSW.Rewards.Shared.DTOs.Leaderboard.LeaderboardUserDto>>>>()))
            .Returns<string, Func<Task<List<SSW.Rewards.Shared.DTOs.Leaderboard.LeaderboardUserDto>>>>((_, factory) => factory());

        var profilePics = new Mock<IProfilePicStorageProvider>();
        profilePics.Setup(x => x.GetProfilePicUri(It.IsAny<string>())).ReturnsAsync(new Uri("https://example.com/default.jpg"));

        _leaderboardService = new LeaderboardService(_db, dateTime.Object, cache.Object, profilePics.Object);

        var mapper = new MapperConfiguration(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly)).CreateMapper();
        _userService = new UserService(
            _db,
            _currentUser.Object,
            cache.Object,
            mapper,
            Options.Create(new UserServiceOptions { StaffSmtpDomain = "ssw.com.au" }),
            NullLogger<UserService>.Instance,
            _leaderboardService);

        var linkAchievement = new Achievement { Name = "Link Company", Value = 0, Type = AchievementType.Linked };
        var company = new SocialMediaPlatform { Name = UserTitles.CompanyPlatformName, Achievement = linkAchievement };
        _db.SocialMediaPlatforms.Add(company);

        _db.Users.Add(NewUser("Viewer", ViewerEmail));
        await _db.SaveChangesAsync();

        _companyPlatformId = company.Id;
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task StaffWithStaffRecordTitle_ShowsStaffTitleEverywhere()
    {
        var user = NewUser("Alice Staff", "alice@ssw.com.au");
        AddCompanyLink(user, "https://www.ssw.com.au");
        _db.Users.Add(user);
        _db.StaffMembers.Add(new StaffMember
        {
            Email = "alice@ssw.com.au",
            Title = "Solution Architect",
            StaffAchievement = new Achievement { Name = "Alice", Value = 100, Type = AchievementType.Scanned }
        });
        await _db.SaveChangesAsync();

        await AssertSameTitleEverywhere(user.Id, "Solution Architect");
    }

    [Test]
    public async Task UserWithCompanyLink_ShowsCompanyDomainEverywhere()
    {
        var user = NewUser("Bob Company", "bob@northwind.com");
        AddCompanyLink(user, "https://www.northwind.com/about");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await AssertSameTitleEverywhere(user.Id, "northwind.com");
    }

    [Test]
    public async Task CommunityUser_ShowsCommunityEverywhere()
    {
        var user = NewUser("Carol Community", "carol@example.com");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await AssertSameTitleEverywhere(user.Id, UserTitles.Community);
    }

    [Test]
    public async Task SoftDeletedStaffRecord_IsIgnored()
    {
        var user = NewUser("Dave Former", "dave@ssw.com.au");
        AddCompanyLink(user, "https://contoso.com");
        _db.Users.Add(user);
        _db.StaffMembers.Add(new StaffMember
        {
            Email = "dave@ssw.com.au",
            Title = "Former Developer",
            DeletedUtc = DateTime.UtcNow,
            StaffAchievement = new Achievement { Name = "Dave", Value = 100, Type = AchievementType.Scanned }
        });
        await _db.SaveChangesAsync();

        await AssertSameTitleEverywhere(user.Id, "contoso.com");
    }

    [Test]
    public async Task StaffRecordWithoutStaffAchievement_IsIgnored()
    {
        var user = NewUser("Erin External", "erin@ssw.com.au");
        _db.Users.Add(user);
        _db.StaffMembers.Add(new StaffMember { Email = "erin@ssw.com.au", Title = "Contractor" });
        await _db.SaveChangesAsync();

        await AssertSameTitleEverywhere(user.Id, UserTitles.Community);
    }

    [TestCase("Solution Architect", "https://northwind.com", ExpectedResult = "Solution Architect")]
    [TestCase("  ", "https://www.northwind.com/", ExpectedResult = "northwind.com")]
    [TestCase(null, "Northwind Traders", ExpectedResult = "Northwind Traders")]
    [TestCase(null, "", ExpectedResult = UserTitles.Community)]
    [TestCase(null, null, ExpectedResult = UserTitles.Community)]
    public string Resolve_AppliesStaffThenCompanyThenCommunity(string? staffTitle, string? companyLink)
        => UserTitles.Resolve(staffTitle, companyLink);

    [Test]
    public void SourceProjection_TranslatesToSingleSqlServerQuery()
    {
        // No connection is opened: ToQueryString only translates.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\unused;Database=unused;Trusted_Connection=True")
            .Options;
        using var sqlDb = new ApplicationDbContext(options);

        var sql = sqlDb.Users
            .Where(u => u.Activated)
            .Select(UserTitles.SourceProjection(sqlDb.StaffMembers))
            .ToQueryString();

        TestContext.Out.WriteLine(sql);
        sql.Should().Contain("[StaffMembers]").And.Contain("[UserSocialMediaIds]").And.Contain("[DeletedUtc] IS NULL");
    }

    private async Task AssertSameTitleEverywhere(int userId, string expected)
    {
        var profile = await _userService.GetUser(userId, CancellationToken.None);

        var feed = await new GetActivitiesQueryHandler(_db, _currentUser.Object)
            .Handle(new GetActivitiesQuery { Filter = ActivityFeedFilter.All, Skip = 0, Take = 50 }, CancellationToken.None);
        var feedItem = feed.Feed.Should().ContainSingle(f => f.UserId == userId).Subject;

        var leaderboard = await _leaderboardService.GetFullLeaderboard(CancellationToken.None);
        var leader = leaderboard.Should().ContainSingle(l => l.UserId == userId).Subject;

        profile.Title.Should().Be(expected, "profile");
        feedItem.UserTitle.Should().Be(expected, "activity feed");
        leader.Title.Should().Be(expected, "leaderboard");
    }

    private void AddCompanyLink(User user, string url)
        => user.SocialMediaIds.Add(new UserSocialMediaId { SocialMediaPlatformId = _companyPlatformId, SocialMediaUserId = url });

    private static User NewUser(string name, string email) => new()
    {
        FullName = name,
        Email = email,
        Activated = true,
        UserAchievements =
        [
            new UserAchievement
            {
                AwardedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Achievement = new Achievement { Name = $"Welcome {name}", Value = 10, Type = AchievementType.Completed }
            }
        ]
    };
}
