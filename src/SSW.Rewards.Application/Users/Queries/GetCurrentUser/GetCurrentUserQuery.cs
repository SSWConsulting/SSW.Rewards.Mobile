using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Extensions;

namespace SSW.Rewards.Application.Users.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<CurrentUserViewModel>
{        
}

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserViewModel>
{
    private readonly ILogger<GetCurrentUserQueryHandler> _logger;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(ILogger<GetCurrentUserQueryHandler> logger, IUserService userService, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _userService = userService;
        _currentUserService = currentUserService;
    }

    public async Task<CurrentUserViewModel> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        CurrentUserViewModel user = await _userService.GetCurrentUser(cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(_currentUserService.GetUserEmail(), "User");
        }

        if (user.IsStaff())
        {
            var code = await _userService.GetStaffQRCode(user.Email, cancellationToken);

            if (!string.IsNullOrWhiteSpace(code))
            {
                user.QRCode = code;
            }
        }

        return user;
    }
}
