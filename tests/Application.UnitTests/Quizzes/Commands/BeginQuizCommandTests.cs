using FluentAssertions;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Quizzes.Commands.BeginQuiz;
using SSW.Rewards.Domain.Entities;
using BeginQuiz = SSW.Rewards.Application.Quizzes.Commands.BeginQuiz;

namespace SSW.Rewards.Application.UnitTests.Quizzes.Commands;

/// <summary>
/// Unit tests for BeginQuizCommand - handles starting a new quiz attempt.
/// Tests quiz existence validation and submission creation.
/// </summary>
[TestFixture]
public class BeginQuizCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private BeginQuiz.Handler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();

        _handler = new BeginQuiz.Handler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _userServiceMock.Object);
    }

    [Test]
    public async Task Handle_WithValidQuiz_ShouldCreateSubmission()
    {
        // Arrange
        const int quizId = 1;
        const int userId = 100;
        const string userEmail = "test@example.com";

        var quiz = new Quiz
        {
            Id = quizId,
            Title = "Test Quiz",
            Description = "A test quiz"
        };

        var completedQuizzes = new List<CompletedQuiz>();

        _contextMock.Setup(x => x.Quizzes.FindAsync(quizId))
            .ReturnsAsync(quiz);

        _currentUserServiceMock.Setup(x => x.GetUserEmail())
            .Returns(userEmail);

        _userServiceMock.Setup(x => x.GetUserId(userEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _contextMock.Setup(x => x.CompletedQuizzes.AddAsync(It.IsAny<CompletedQuiz>(), It.IsAny<CancellationToken>()))
            .Callback<CompletedQuiz, CancellationToken>((cq, _) =>
            {
                cq.Id = 42; // Simulate database generating ID
                completedQuizzes.Add(cq);
            })
            .Returns((CompletedQuiz cq, CancellationToken _) => ValueTask.FromResult((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CompletedQuiz>)null!));

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new BeginQuizCommand { QuizId = quizId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(42);
        completedQuizzes.Should().HaveCount(1);
        completedQuizzes[0].QuizId.Should().Be(quizId);
        completedQuizzes[0].UserId.Should().Be(userId);

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_WithNonExistentQuiz_ShouldThrowNotFoundException()
    {
        // Arrange
        const int nonExistentQuizId = 999;

        _contextMock.Setup(x => x.Quizzes.FindAsync(nonExistentQuizId))
            .ReturnsAsync((Quiz?)null);

        var command = new BeginQuizCommand { QuizId = nonExistentQuizId };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{nameof(Quiz)}*{nonExistentQuizId}*");

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithDifferentUser_ShouldUseCorrectUserId()
    {
        // Arrange
        const int quizId = 5;
        const int userId = 200;
        const string userEmail = "another@example.com";

        var quiz = new Quiz { Id = quizId, Title = "Another Quiz" };
        CompletedQuiz? capturedQuiz = null;

        _contextMock.Setup(x => x.Quizzes.FindAsync(quizId))
            .ReturnsAsync(quiz);

        _currentUserServiceMock.Setup(x => x.GetUserEmail())
            .Returns(userEmail);

        _userServiceMock.Setup(x => x.GetUserId(userEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _contextMock.Setup(x => x.CompletedQuizzes.AddAsync(It.IsAny<CompletedQuiz>(), It.IsAny<CancellationToken>()))
            .Callback<CompletedQuiz, CancellationToken>((cq, _) =>
            {
                cq.Id = 100;
                capturedQuiz = cq;
            })
            .Returns((CompletedQuiz cq, CancellationToken _) => ValueTask.FromResult((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CompletedQuiz>)null!));

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new BeginQuizCommand { QuizId = quizId };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedQuiz.Should().NotBeNull();
        capturedQuiz!.UserId.Should().Be(userId);

        _userServiceMock.Verify(x => x.GetUserId(userEmail, It.IsAny<CancellationToken>()), Times.Once);
    }
}

