using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;
using SSW.Rewards.Domain.Entities;
using SubmitAnswerCommand = SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;

namespace SSW.Rewards.Application.UnitTests.Quizzes.Commands;

/// <summary>
/// Unit tests for SubmitAnswerCommand - handles individual answer submission to QuizGPT.
/// Tests question validation, answer retrieval, and GPT service interaction.
/// </summary>
[TestFixture]
public class SubmitAnswerCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IQuizGPTService> _quizGptServiceMock = null!;
    private Mock<ILogger<SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand.SubmitAnswerCommand>> _loggerMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private SubmitAnswerCommand.Handler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _quizGptServiceMock = new Mock<IQuizGPTService>();
        _loggerMock = new Mock<ILogger<SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand.SubmitAnswerCommand>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();

        _handler = new SubmitAnswerCommand.Handler(
            _contextMock.Object,
            _quizGptServiceMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _userServiceMock.Object);
    }

    [Test]
    public async Task Handle_WithValidQuestion_ShouldProcessAnswer()
    {
        // Arrange
        const int submissionId = 1;
        const int questionId = 10;
        const int userId = 100;
        const string userEmail = "test@example.com";
        const string answerText = "The answer is 42";
        const string questionText = "What is the meaning of life?";
        const string correctAnswer = "42";

        var question = new QuizQuestion
        {
            Id = questionId,
            Text = questionText
        };

        var quizAnswer = new QuizAnswer
        {
            Id = 1,
            QuestionId = questionId,
            Text = correctAnswer,
            IsCorrect = true
        };

        SetupQuestionQuery(question);
        SetupAnswerQuery(quizAnswer);
        SetupUserMocks(userId, userEmail);

        var command = new SubmitAnswerCommand.SubmitAnswerCommand
        {
            SubmissionId = submissionId,
            QuestionId = questionId,
            AnswerText = answerText
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _quizGptServiceMock.Verify(x => x.ProcessAnswer(
            userId,
            It.Is<QuizGPTRequestDto>(dto =>
                dto.QuestionText == questionText &&
                dto.AnswerText == answerText &&
                dto.BenchmarkAnswer == correctAnswer),
            command), Times.Once);
    }

    [Test]
    public void Handle_WithNonExistentQuestion_ShouldThrowNotFoundException()
    {
        // Arrange
        const int questionId = 999;
        const string userEmail = "test@example.com";

        SetupQuestionQuery(null);
        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(userEmail);

        var command = new SubmitAnswerCommand.SubmitAnswerCommand
        {
            SubmissionId = 1,
            QuestionId = questionId,
            AnswerText = "Some answer"
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{nameof(QuizQuestion)}*{questionId}*");

        _quizGptServiceMock.Verify(x => x.ProcessAnswer(
            It.IsAny<int>(),
            It.IsAny<QuizGPTRequestDto>(),
            It.IsAny<SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand.SubmitAnswerCommand>()), Times.Never);
    }

    [Test]
    public void Handle_WithNullQuestionText_ShouldThrowArgumentNullException()
    {
        // Arrange
        const int questionId = 20;
        const string userEmail = "test@example.com";

        var question = new QuizQuestion
        {
            Id = questionId,
            Text = null // Null question text
        };

        SetupQuestionQuery(question);
        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(userEmail);

        var command = new SubmitAnswerCommand.SubmitAnswerCommand
        {
            SubmissionId = 1,
            QuestionId = questionId,
            AnswerText = "Some answer"
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Handle_WithNoCorrectAnswer_ShouldProcessWithNullBenchmark()
    {
        // Arrange
        const int submissionId = 2;
        const int questionId = 30;
        const int userId = 101;
        const string userEmail = "user@example.com";
        const string answerText = "Open ended answer";
        const string questionText = "What do you think?";

        var question = new QuizQuestion
        {
            Id = questionId,
            Text = questionText
        };

        SetupQuestionQuery(question);
        SetupAnswerQuery(null); // No correct answer
        SetupUserMocks(userId, userEmail);

        var command = new SubmitAnswerCommand.SubmitAnswerCommand
        {
            SubmissionId = submissionId,
            QuestionId = questionId,
            AnswerText = answerText
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _quizGptServiceMock.Verify(x => x.ProcessAnswer(
            userId,
            It.Is<QuizGPTRequestDto>(dto =>
                dto.QuestionText == questionText &&
                dto.AnswerText == answerText &&
                dto.BenchmarkAnswer == null),
            command), Times.Once);
    }

    [Test]
    public async Task Handle_WithEmptyAnswerText_ShouldStillProcess()
    {
        // Arrange
        const int questionId = 40;
        const int userId = 102;
        const string userEmail = "empty@example.com";
        const string questionText = "Test question";

        var question = new QuizQuestion
        {
            Id = questionId,
            Text = questionText
        };

        var quizAnswer = new QuizAnswer
        {
            Id = 1,
            QuestionId = questionId,
            Text = "Correct answer",
            IsCorrect = true
        };

        SetupQuestionQuery(question);
        SetupAnswerQuery(quizAnswer);
        SetupUserMocks(userId, userEmail);

        var command = new SubmitAnswerCommand.SubmitAnswerCommand
        {
            SubmissionId = 1,
            QuestionId = questionId,
            AnswerText = string.Empty
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _quizGptServiceMock.Verify(x => x.ProcessAnswer(
            It.IsAny<int>(),
            It.Is<QuizGPTRequestDto>(dto => dto.AnswerText == string.Empty),
            command), Times.Once);
    }

    // Helper methods
    private void SetupQuestionQuery(QuizQuestion? question)
    {
        var questions = question != null ? new List<QuizQuestion> { question } : new List<QuizQuestion>();
        var mockDbSet = questions.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.QuizQuestions).Returns(mockDbSet.Object);
    }

    private void SetupAnswerQuery(QuizAnswer? answer)
    {
        var answers = answer != null ? new List<QuizAnswer> { answer } : new List<QuizAnswer>();
        var mockDbSet = answers.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.QuizAnswers).Returns(mockDbSet.Object);
    }

    private void SetupUserMocks(int userId, string userEmail)
    {
        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(userEmail);
        _userServiceMock.Setup(x => x.GetUserId(userEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);
    }
}

