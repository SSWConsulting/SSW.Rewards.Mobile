using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.UnitTests.Quizzes.Commands;

/// <summary>
/// Unit tests for SubmitUserQuizCommandValidator - a passed quiz blocks re-submission
/// for that user only, not for everyone (#1583).
/// </summary>
[TestFixture]
public class SubmitUserQuizCommandValidatorTests
{
    private const int QuizId = 1;
    private const int UserAId = 100;
    private const int UserBId = 200;
    private const string UserAEmail = "user.a@example.com";
    private const string UserBEmail = "user.b@example.com";

    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private SubmitUserQuizCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();

        _userServiceMock.Setup(x => x.GetUserId(UserAEmail, It.IsAny<CancellationToken>())).ReturnsAsync(UserAId);
        _userServiceMock.Setup(x => x.GetUserId(UserBEmail, It.IsAny<CancellationToken>())).ReturnsAsync(UserBId);

        _validator = new SubmitUserQuizCommandValidator(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _userServiceMock.Object);
    }

    [Test]
    public async Task Validate_WhenAnotherUserPassedQuiz_ShouldAllowSubmission()
    {
        // Arrange - user A has passed the quiz, user B has never attempted it
        SetupCompletedQuizzes(new CompletedQuiz { QuizId = QuizId, UserId = UserAId, Passed = true });
        SetupCurrentUser(UserBEmail);

        // Act
        var result = await _validator.ValidateAsync(CreateCommand());

        // Assert
        result.IsValid.Should().BeTrue("another user's pass must not block this user");
    }

    [Test]
    public async Task Validate_WhenCurrentUserAlreadyPassedQuiz_ShouldRejectSubmission()
    {
        // Arrange
        SetupCompletedQuizzes(new CompletedQuiz { QuizId = QuizId, UserId = UserAId, Passed = true });
        SetupCurrentUser(UserAEmail);

        // Act
        var result = await _validator.ValidateAsync(CreateCommand());

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(SubmitUserQuizCommand.QuizId));
    }

    [Test]
    public async Task Validate_WhenCurrentUserOnlyFailedQuiz_ShouldAllowSubmission()
    {
        // Arrange
        SetupCompletedQuizzes(new CompletedQuiz { QuizId = QuizId, UserId = UserAId, Passed = false });
        SetupCurrentUser(UserAEmail);

        // Act
        var result = await _validator.ValidateAsync(CreateCommand());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public async Task Validate_WhenCurrentUserPassedDifferentQuiz_ShouldAllowSubmission()
    {
        // Arrange
        SetupCompletedQuizzes(new CompletedQuiz { QuizId = QuizId + 1, UserId = UserAId, Passed = true });
        SetupCurrentUser(UserAEmail);

        // Act
        var result = await _validator.ValidateAsync(CreateCommand());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private void SetupCompletedQuizzes(params CompletedQuiz[] completedQuizzes)
    {
        var mockDbSet = completedQuizzes.ToList().BuildMockDbSet();
        _contextMock.Setup(x => x.CompletedQuizzes).Returns(mockDbSet.Object);
    }

    private void SetupCurrentUser(string email)
        => _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(email);

    private static SubmitUserQuizCommand CreateCommand() => new()
    {
        QuizId = QuizId,
        Answers = new List<SubmittedAnswerDto> { new() { QuestionId = 1, SelectedAnswerId = 1 } }
    };
}
