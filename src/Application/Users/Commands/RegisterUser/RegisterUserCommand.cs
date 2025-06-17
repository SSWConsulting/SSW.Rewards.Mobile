﻿namespace SSW.Rewards.Application.Users.Commands.RegisterUser;

public class RegisterUserCommand : IRequest;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public RegisterUserCommandHandler(ICurrentUserService currentUserService, IUserService userService)
    {
        _currentUserService = currentUserService;
        _userService = userService;
    }

    public async Task Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var newUser = new User
        {
            Email = _currentUserService.GetUserEmail(),
            FullName = _currentUserService.GetUserFullName(),
            // TODO: test removing. Don't see how this could be here at this point? But I may have added it to the mapping profile for a reason. 
            Avatar = _currentUserService.GetUserProfilePic(),
            CreatedUtc = DateTime.UtcNow
        };

        await _userService.CreateUser(newUser, cancellationToken);
    }
}
