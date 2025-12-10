using FluentAssertions;
using Moq;
using NUnit.Framework;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Users.Commands.RegisterUser;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.UnitTests.Users.Commands;

/// <summary>
/// Unit tests for RegisterUserCommand - handles new user registration.
/// Tests user creation, email assignment, and profile picture handling.
/// </summary>
[TestFixture]
public class RegisterUserCommandTests
{
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private RegisterUserCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();

        _handler = new RegisterUserCommandHandler(
            _currentUserServiceMock.Object,
            _userServiceMock.Object);
    }

    [Test]
    public async Task Handle_WithValidUser_ShouldCreateUserWithCorrectDetails()
    {
        // Arrange
        const string userEmail = "newuser@example.com";
        const string fullName = "John Doe";
        const string avatarUrl = "https://example.com/avatar.jpg";

        User? capturedUser = null;

        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(userEmail);
        _currentUserServiceMock.Setup(x => x.GetUserFullName()).Returns(fullName);
        _currentUserServiceMock.Setup(x => x.GetUserProfilePic()).Returns(avatarUrl);

        _userServiceMock.Setup(x => x.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, ct) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken ct) => user);

        var command = new RegisterUserCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(userEmail);
        capturedUser.FullName.Should().Be(fullName);
        capturedUser.Avatar.Should().Be(avatarUrl);
        capturedUser.CreatedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _userServiceMock.Verify(x => x.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithNoAvatar_ShouldCreateUserWithNullAvatar()
    {
        // Arrange
        const string userEmail = "noavatar@example.com";
        const string fullName = "Jane Smith";

        User? capturedUser = null;

        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(userEmail);
        _currentUserServiceMock.Setup(x => x.GetUserFullName()).Returns(fullName);
        _currentUserServiceMock.Setup(x => x.GetUserProfilePic()).Returns((string?)null);

        _userServiceMock.Setup(x => x.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, ct) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken ct) => user);

        var command = new RegisterUserCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Avatar.Should().BeNull();
    }

    [Test]
    public async Task Handle_ShouldCallCurrentUserServiceMethods()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns("test@example.com");
        _currentUserServiceMock.Setup(x => x.GetUserFullName()).Returns("Test User");
        _currentUserServiceMock.Setup(x => x.GetUserProfilePic()).Returns("avatar.jpg");

        _userServiceMock.Setup(x => x.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        var command = new RegisterUserCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetUserEmail(), Times.Once);
        _currentUserServiceMock.Verify(x => x.GetUserFullName(), Times.Once);
        _currentUserServiceMock.Verify(x => x.GetUserProfilePic(), Times.Once);
    }

    [Test]
    public async Task Handle_WithDifferentUsers_ShouldCreateDifferentUserObjects()
    {
        // Arrange
        const string email1 = "user1@example.com";
        const string email2 = "user2@example.com";
        const string name1 = "User One";
        const string name2 = "User Two";

        var capturedUsers = new List<User>();

        _userServiceMock.Setup(x => x.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, ct) => capturedUsers.Add(user))
            .ReturnsAsync((User user, CancellationToken ct) => user);

        var command = new RegisterUserCommand();

        // Act - User 1
        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(email1);
        _currentUserServiceMock.Setup(x => x.GetUserFullName()).Returns(name1);
        _currentUserServiceMock.Setup(x => x.GetUserProfilePic()).Returns("avatar1.jpg");
        await _handler.Handle(command, CancellationToken.None);

        // Act - User 2
        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(email2);
        _currentUserServiceMock.Setup(x => x.GetUserFullName()).Returns(name2);
        _currentUserServiceMock.Setup(x => x.GetUserProfilePic()).Returns("avatar2.jpg");
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUsers.Should().HaveCount(2);
        capturedUsers[0].Email.Should().Be(email1);
        capturedUsers[0].FullName.Should().Be(name1);
        capturedUsers[1].Email.Should().Be(email2);
        capturedUsers[1].FullName.Should().Be(name2);
    }

    [Test]
    public async Task Handle_ShouldSetCreatedUtcToCurrentTime()
    {
        // Arrange
        var beforeTime = DateTime.UtcNow;
        User? capturedUser = null;

        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns("test@example.com");
        _currentUserServiceMock.Setup(x => x.GetUserFullName()).Returns("Test User");
        _currentUserServiceMock.Setup(x => x.GetUserProfilePic()).Returns("avatar.jpg");

        _userServiceMock.Setup(x => x.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, ct) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken ct) => user);

        var command = new RegisterUserCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        var afterTime = DateTime.UtcNow;

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.CreatedUtc.Should().BeOnOrAfter(beforeTime);
        capturedUser.CreatedUtc.Should().BeOnOrBefore(afterTime);
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldPassToUserService()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _currentUserServiceMock.Setup(x => x.GetUserEmail()).Returns("test@example.com");
        _currentUserServiceMock.Setup(x => x.GetUserFullName()).Returns("Test User");
        _currentUserServiceMock.Setup(x => x.GetUserProfilePic()).Returns("avatar.jpg");

        _userServiceMock.Setup(x => x.CreateUser(It.IsAny<User>(), cancellationToken))
            .ReturnsAsync(new User());

        var command = new RegisterUserCommand();

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _userServiceMock.Verify(x => x.CreateUser(It.IsAny<User>(), cancellationToken), Times.Once);
    }
}

