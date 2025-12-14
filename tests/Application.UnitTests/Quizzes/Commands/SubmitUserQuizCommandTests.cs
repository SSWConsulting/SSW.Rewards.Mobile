using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Quizzes;
using SubmitUserQuiz = SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;

namespace SSW.Rewards.Application.UnitTests.Quizzes.Commands;

/// <summary>
/// Unit tests for SubmitUserQuizCommand - handles quiz submission and grading.
/// Tests answer validation, scoring logic, achievement awarding, and edge cases.
/// </summary>
[TestFixture]
public class SubmitUserQuizCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private Mock<DbSet<Quiz>> _quizDbSetMock = null!;
    private Mock<DbSet<UserAchievement>> _userAchievementsDbSetMock = null!;
    private Mock<DbSet<CompletedQuiz>> _completedQuizzesDbSetMock = null!;
    private SubmitUserQuiz.Handler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();
        _quizDbSetMock = new Mock<DbSet<Quiz>>();
        _userAchievementsDbSetMock = new Mock<DbSet<UserAchievement>>();
        _completedQuizzesDbSetMock = new Mock<DbSet<CompletedQuiz>>();

        _contextMock.Setup(x => x.Quizzes).Returns(_quizDbSetMock.Object);
        _contextMock.Setup(x => x.UserAchievements).Returns(_userAchievementsDbSetMock.Object);
        _contextMock.Setup(x => x.CompletedQuizzes).Returns(_completedQuizzesDbSetMock.Object);

        _handler = new SubmitUserQuiz.Handler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _userServiceMock.Object);
    }

    [Test]
    public async Task Handle_WithAllCorrectAnswers_ShouldPassAndAwardPoints()
    {
        // Arrange
        const int quizId = 1;
        const int userId = 100;
        const int achievementId = 10;
        const int points = 50;
        const string userEmail = "test@example.com";

        var achievement = new Achievement
        {
            Id = achievementId,
            Value = points,
            Code = "QUIZ_ACHIEVEMENT",
            Name = "Quiz Achievement",
            Type = AchievementType.Linked
        };

        var quiz = CreateQuizWithQuestions(quizId, achievementId, achievement);
        var command = CreateCommandWithCorrectAnswers(quizId);

        SetupMocksForQuery(quiz);
        SetupUserMocks(userId, userEmail);

        var capturedAchievements = new List<UserAchievement>();
        var capturedCompletedQuizzes = new List<CompletedQuiz>();

        _userAchievementsDbSetMock.Setup(x => x.Add(It.IsAny<UserAchievement>()))
            .Callback<UserAchievement>(capturedAchievements.Add);

        _completedQuizzesDbSetMock.Setup(x => x.Add(It.IsAny<CompletedQuiz>()))
            .Callback<CompletedQuiz>(capturedCompletedQuizzes.Add);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.QuizId.Should().Be(quizId);
        result.Passed.Should().BeTrue();
        result.Points.Should().Be(points);
        result.Results.Should().HaveCount(2);
        result.Results.Should().AllSatisfy(r => r.Correct.Should().BeTrue());

        capturedAchievements.Should().HaveCount(1);
        capturedAchievements[0].UserId.Should().Be(userId);
        capturedAchievements[0].AchievementId.Should().Be(achievementId);

        capturedCompletedQuizzes.Should().HaveCount(1);
        capturedCompletedQuizzes[0].Passed.Should().BeTrue();
        capturedCompletedQuizzes[0].Answers.Should().HaveCount(2);
    }

    [Test]
    public async Task Handle_WithSomeIncorrectAnswers_ShouldFailAndNotAwardPoints()
    {
        // Arrange
        const int quizId = 2;
        const int userId = 101;
        const int achievementId = 11;
        const string userEmail = "user@example.com";

        var achievement = new Achievement
        {
            Id = achievementId,
            Value = 100,
            Code = "QUIZ_ACHIEVEMENT_2",
            Name = "Quiz Achievement 2",
            Type = AchievementType.Linked
        };

        var quiz = CreateQuizWithQuestions(quizId, achievementId, achievement);
        var command = CreateCommandWithIncorrectAnswers(quizId);

        SetupMocksForQuery(quiz);
        SetupUserMocks(userId, userEmail);

        var capturedAchievements = new List<UserAchievement>();
        var capturedCompletedQuizzes = new List<CompletedQuiz>();

        _userAchievementsDbSetMock.Setup(x => x.Add(It.IsAny<UserAchievement>()))
            .Callback<UserAchievement>(capturedAchievements.Add);

        _completedQuizzesDbSetMock.Setup(x => x.Add(It.IsAny<CompletedQuiz>()))
            .Callback<CompletedQuiz>(capturedCompletedQuizzes.Add);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.QuizId.Should().Be(quizId);
        result.Passed.Should().BeFalse();
        result.Results.Should().HaveCount(2);
        result.Results.Should().Contain(r => !r.Correct);

        capturedAchievements.Should().BeEmpty("no achievement should be awarded for failed quiz");

        capturedCompletedQuizzes.Should().HaveCount(1);
        capturedCompletedQuizzes[0].Passed.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WithSingleQuestion_ShouldHandleCorrectly()
    {
        // Arrange
        const int quizId = 3;
        const int userId = 102;
        const int achievementId = 12;
        const string userEmail = "single@example.com";

        var achievement = new Achievement
        {
            Id = achievementId,
            Value = 25,
            Code = "SINGLE_Q",
            Name = "Single Question Quiz",
            Type = AchievementType.Linked
        };

        var quiz = CreateQuizWithSingleQuestion(quizId, achievementId, achievement);
        var command = new SubmitUserQuizCommand
        {
            QuizId = quizId,
            Answers = new List<SubmittedAnswerDto>
            {
                new() { QuestionId = 1, SelectedAnswerId = 1 } // Correct answer
            }
        };

        SetupMocksForQuery(quiz);
        SetupUserMocks(userId, userEmail);

        var capturedAchievements = new List<UserAchievement>();
        var capturedCompletedQuizzes = new List<CompletedQuiz>();

        _userAchievementsDbSetMock.Setup(x => x.Add(It.IsAny<UserAchievement>()))
            .Callback<UserAchievement>(capturedAchievements.Add);

        _completedQuizzesDbSetMock.Setup(x => x.Add(It.IsAny<CompletedQuiz>()))
            .Callback<CompletedQuiz>(capturedCompletedQuizzes.Add);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Passed.Should().BeTrue();
        result.Results.Should().HaveCount(1);
        capturedAchievements.Should().HaveCount(1);
    }

    [Test]
    public async Task Handle_MultipleTimes_ShouldCreateMultipleCompletedQuizzes()
    {
        // Arrange
        const int quizId = 4;
        const int userId = 103;
        const int achievementId = 13;
        const string userEmail = "repeat@example.com";

        var achievement = new Achievement
        {
            Id = achievementId,
            Value = 75,
            Code = "REPEAT_QUIZ",
            Name = "Repeatable Quiz",
            Type = AchievementType.Linked
        };

        var quiz = CreateQuizWithQuestions(quizId, achievementId, achievement);
        var command = CreateCommandWithCorrectAnswers(quizId);

        SetupMocksForQuery(quiz);
        SetupUserMocks(userId, userEmail);

        var capturedCompletedQuizzes = new List<CompletedQuiz>();

        _userAchievementsDbSetMock.Setup(x => x.Add(It.IsAny<UserAchievement>()));
        _completedQuizzesDbSetMock.Setup(x => x.Add(It.IsAny<CompletedQuiz>()))
            .Callback<CompletedQuiz>(capturedCompletedQuizzes.Add);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedCompletedQuizzes.Should().HaveCount(2, "each submission should be recorded");
    }

    // Helper methods
    private Quiz CreateQuizWithQuestions(int quizId, int achievementId, Achievement achievement)
    {
        var question1 = new QuizQuestion
        {
            Id = 1,
            Text = "What is 2+2?",
            Answers = new List<QuizAnswer>
            {
                new() { Id = 1, Text = "4", IsCorrect = true },
                new() { Id = 2, Text = "5", IsCorrect = false }
            }
        };

        var question2 = new QuizQuestion
        {
            Id = 2,
            Text = "What is the capital of France?",
            Answers = new List<QuizAnswer>
            {
                new() { Id = 3, Text = "London", IsCorrect = false },
                new() { Id = 4, Text = "Paris", IsCorrect = true }
            }
        };

        return new Quiz
        {
            Id = quizId,
            Title = "Test Quiz",
            Description = "A test quiz",
            AchievementId = achievementId,
            Achievement = achievement,
            Questions = new List<QuizQuestion> { question1, question2 }
        };
    }

    private Quiz CreateQuizWithSingleQuestion(int quizId, int achievementId, Achievement achievement)
    {
        var question = new QuizQuestion
        {
            Id = 1,
            Text = "Single question?",
            Answers = new List<QuizAnswer>
            {
                new() { Id = 1, Text = "Correct", IsCorrect = true },
                new() { Id = 2, Text = "Wrong", IsCorrect = false }
            }
        };

        return new Quiz
        {
            Id = quizId,
            Title = "Single Question Quiz",
            Description = "One question quiz",
            AchievementId = achievementId,
            Achievement = achievement,
            Questions = new List<QuizQuestion> { question }
        };
    }

    private SubmitUserQuizCommand CreateCommandWithCorrectAnswers(int quizId)
    {
        return new SubmitUserQuizCommand
        {
            QuizId = quizId,
            Answers = new List<SubmittedAnswerDto>
            {
                new() { QuestionId = 1, SelectedAnswerId = 1 }, // Correct: 4
                new() { QuestionId = 2, SelectedAnswerId = 4 }  // Correct: Paris
            }
        };
    }

    private SubmitUserQuizCommand CreateCommandWithIncorrectAnswers(int quizId)
    {
        return new SubmitUserQuizCommand
        {
            QuizId = quizId,
            Answers = new List<SubmittedAnswerDto>
            {
                new() { QuestionId = 1, SelectedAnswerId = 2 }, // Incorrect: 5
                new() { QuestionId = 2, SelectedAnswerId = 4 }  // Correct: Paris
            }
        };
    }

    private void SetupMocksForQuery(Quiz quiz)
    {
        var mockDbSet = new List<Quiz> { quiz }.BuildMockDbSet();
        _contextMock.Setup(x => x.Quizzes).Returns(mockDbSet.Object);
    }

    private void SetupUserMocks(int userId, string userEmail)
    {
        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(userEmail);
        _userServiceMock.Setup(x => x.GetUserId(userEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);
    }
}

