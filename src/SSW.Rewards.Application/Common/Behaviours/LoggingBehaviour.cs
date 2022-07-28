using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUserService, IUserService userService)//, IIdentityService identityService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _userService = userService;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        string userName = string.Empty;
        int userId = 0;

        var userEmail = _currentUserService.GetUserId() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            userId = await _userService.GetUserId(userEmail);

            if (userId > 0)
            {
                var user = await _userService.GetUser(userId, cancellationToken);

                userName = user.FullName;
            }
        }

        _logger.LogInformation("SSW.Rewards Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);
    }
}
