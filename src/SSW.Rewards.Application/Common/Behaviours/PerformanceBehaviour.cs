using System.Diagnostics;
using SSW.Rewards.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    //private readonly IIdentityService _identityService;

    public PerformanceBehaviour(
        ILogger<TRequest> logger,
        ICurrentUserService currentUserService,
        //IIdentityService identityService)
        IUserService userService)
    {
        _timer = new Stopwatch();

        _logger = logger;
        _currentUserService = currentUserService;
        _userService = userService;
        //_identityService = identityService;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
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

            _logger.LogWarning("SSW.Rewards Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                requestName, elapsedMilliseconds, userId, userName, request);
        }

        return response;
    }
}
