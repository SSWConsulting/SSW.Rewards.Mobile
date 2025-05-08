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
        int userId = 0;

        var userEmail = _currentUserService.GetUserEmail() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            try
            {
                userId = await _userService.GetUserId(userEmail, cancellationToken);
            }
            catch (Exception)
            {
                userId = 0;
            }
        }

        _logger.LogInformation("SSW.Rewards Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userEmail, request);
    }
}
