using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Enums;
using GetQuizListForUser = SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUser;

namespace SSW.Rewards.Application.UnitTests.Quizzes.Queries;

/// <summary>
/// Unit tests for GetQuizListForUser query - retrieves available quizzes for a user.
/// Tests quiz filtering, completion tracking, and data projection.
/// </summary>
[TestFixture]
public class GetQuizListForUserTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private GetQuizListForUser.Handler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();

        _handler = new GetQuizListForUser.Handler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _userServiceMock.Object);
    }

    [Test]
    public async Task Handle_WithNoQuizzes_ShouldReturnEmptyList()
    {
        // Arrange
        const int userId = 100;
        const string userEmail = "test@example.com";

        SetupQuizzesQuery(new List<Quiz>());
        SetupCompletedQuizzesQuery(new List<CompletedQuiz>());
        SetupUserMocks(userId, userEmail);

        var query = new GetQuizListForUser.GetQuizListForUser();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_WithActiveQuizzes_ShouldReturnAll()
    {
        // Arrange
        const int userId = 100;
        const string userEmail = "test@example.com";

        var quizzes = new List<Quiz>
        {
            CreateQuiz(1, "Quiz 1", "Description 1", 50, false),
            CreateQuiz(2, "Quiz 2", "Description 2", 75, false),
            CreateQuiz(3, "Quiz 3", "Description 3", 100, false)
        };

        SetupQuizzesQuery(quizzes);
        SetupCompletedQuizzesQuery(new List<CompletedQuiz>());
        SetupUserMocks(userId, userEmail);

        var query = new GetQuizListForUser.GetQuizListForUser();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(q => q.Passed.Should().BeFalse());
    }

    [Test]
    public async Task Handle_WithArchivedQuizzes_ShouldExcludeArchived()
    {
        // Arrange
        const int userId = 100;
        const string userEmail = "test@example.com";

        var quizzes = new List<Quiz>
        {
            CreateQuiz(1, "Active Quiz", "Active", 50, false),
            CreateQuiz(2, "Archived Quiz", "Archived", 75, true)
        };

        SetupQuizzesQuery(quizzes);
        SetupCompletedQuizzesQuery(new List<CompletedQuiz>());
        SetupUserMocks(userId, userEmail);

        var query = new GetQuizListForUser.GetQuizListForUser();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Active Quiz");
    }

    [Test]
    public async Task Handle_WithCompletedQuizzes_ShouldMarkAsPassed()
    {
        // Arrange
        const int userId = 100;
        const string userEmail = "test@example.com";

        var quizzes = new List<Quiz>
        {
            CreateQuiz(1, "Completed Quiz", "Completed", 50, false),
            CreateQuiz(2, "Incomplete Quiz", "Incomplete", 75, false)
        };

        var completedQuizzes = new List<CompletedQuiz>
        {
            new() { QuizId = 1, UserId = userId, Passed = true }
        };

        SetupQuizzesQuery(quizzes);
        SetupCompletedQuizzesQuery(completedQuizzes);
        SetupUserMocks(userId, userEmail);

        var query = new GetQuizListForUser.GetQuizListForUser();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.First(q => q.Id == 1).Passed.Should().BeTrue();
        resultList.First(q => q.Id == 2).Passed.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WithMultipleCompletions_ShouldStillMarkAsPassed()
    {
        // Arrange
        const int userId = 100;
        const string userEmail = "test@example.com";

        var quizzes = new List<Quiz>
        {
            CreateQuiz(1, "Popular Quiz", "Popular", 50, false)
        };

        var completedQuizzes = new List<CompletedQuiz>
        {
            new() { QuizId = 1, UserId = userId, Passed = true },
            new() { QuizId = 1, UserId = userId, Passed = true }
        };

        SetupQuizzesQuery(quizzes);
        SetupCompletedQuizzesQuery(completedQuizzes);
        SetupUserMocks(userId, userEmail);

        var query = new GetQuizListForUser.GetQuizListForUser();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Passed.Should().BeTrue();
    }

    [Test]
    public async Task Handle_ShouldReturnQuizzesInAlphabeticalOrder()
    {
        // Arrange
        const int userId = 100;
        const string userEmail = "test@example.com";

        var quizzes = new List<Quiz>
        {
            CreateQuiz(1, "Zebra Quiz", "Z", 50, false),
            CreateQuiz(2, "Apple Quiz", "A", 75, false),
            CreateQuiz(3, "Banana Quiz", "B", 100, false)
        };

        SetupQuizzesQuery(quizzes);
        SetupCompletedQuizzesQuery(new List<CompletedQuiz>());
        SetupUserMocks(userId, userEmail);

        var query = new GetQuizListForUser.GetQuizListForUser();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList[0].Title.Should().Be("Apple Quiz");
        resultList[1].Title.Should().Be("Banana Quiz");
        resultList[2].Title.Should().Be("Zebra Quiz");
    }

    [Test]
    public async Task Handle_ShouldIncludeCarouselProperties()
    {
        // Arrange
        const int userId = 100;
        const string userEmail = "test@example.com";

        var quizzes = new List<Quiz>
        {
            new()
            {
                Id = 1,
                Title = "Carousel Quiz",
                Description = "Featured quiz",
                IsCarousel = true,
                CarouselImage = "carousel.jpg",
                ThumbnailImage = "thumb.jpg",
                Icon = Icons.Trophy,
                IsArchived = false,
                Questions = new List<QuizQuestion>(),
                Achievement = new Achievement
                {
                    Id = 1,
                    Value = 100,
                    Code = "CAROUSEL",
                    Name = "Carousel Achievement",
                    Type = AchievementType.Linked
                }
            }
        };

        SetupQuizzesQuery(quizzes);
        SetupCompletedQuizzesQuery(new List<CompletedQuiz>());
        SetupUserMocks(userId, userEmail);

        var query = new GetQuizListForUser.GetQuizListForUser();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var quiz = result.First();
        quiz.IsCarousel.Should().BeTrue();
        quiz.CarouselImage.Should().Be("carousel.jpg");
        quiz.ThumbnailImage.Should().Be("thumb.jpg");
        quiz.Points.Should().Be(100);
    }

    [Test]
    public async Task Handle_WithFailedAttempt_ShouldNotMarkAsPassed()
    {
        // Arrange
        const int userId = 100;
        const string userEmail = "test@example.com";

        var quizzes = new List<Quiz>
        {
            CreateQuiz(1, "Failed Quiz", "Failed", 50, false)
        };

        var completedQuizzes = new List<CompletedQuiz>
        {
            new() { QuizId = 1, UserId = userId, Passed = false }
        };

        SetupQuizzesQuery(quizzes);
        SetupCompletedQuizzesQuery(completedQuizzes);
        SetupUserMocks(userId, userEmail);

        var query = new GetQuizListForUser.GetQuizListForUser();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.First().Passed.Should().BeFalse();
    }

    // Helper methods
    private Quiz CreateQuiz(int id, string title, string description, int points, bool isArchived)
    {
        return new Quiz
        {
            Id = id,
            Title = title,
            Description = description,
            Icon = Icons.Trophy,
            IsArchived = isArchived,
            ThumbnailImage = $"thumb_{id}.jpg",
            CarouselImage = $"carousel_{id}.jpg",
            IsCarousel = false,
            Questions = new List<QuizQuestion>(),
            Achievement = new Achievement
            {
                Id = id,
                Value = points,
                Code = $"QUIZ_{id}",
                Name = $"Achievement {id}",
                Type = AchievementType.Linked
            }
        };
    }

    private void SetupQuizzesQuery(List<Quiz> quizzes)
    {
        var mockDbSet = quizzes.BuildMockDbSet();
        _contextMock.Setup(x => x.Quizzes).Returns(mockDbSet.Object);
    }

    private void SetupCompletedQuizzesQuery(List<CompletedQuiz> completedQuizzes)
    {
        var mockDbSet = completedQuizzes.BuildMockDbSet();
        _contextMock.Setup(x => x.CompletedQuizzes).Returns(mockDbSet.Object);
    }

    private void SetupUserMocks(int userId, string userEmail)
    {
        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(userEmail);
        _userServiceMock.Setup(x => x.GetUserId(userEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);
    }
}

