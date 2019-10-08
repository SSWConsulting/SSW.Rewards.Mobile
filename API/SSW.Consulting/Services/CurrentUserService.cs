using MediatR;
using Microsoft.AspNetCore.Http;
using SSW.Consulting.Application.Common.Exceptions;
using SSW.Consulting.Application.Common.Interfaces;
using SSW.Consulting.Application.User.Commands.UpsertCurrentUser;
using SSW.Consulting.Application.User.Queries.GetCurrentUser;
using SSW.Consulting.Application.User.Queries.GetUser;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.WebAPI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IMediator _mediatr;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private CurrentUserViewModel _currentUser;

        public CurrentUserService(
            IMediator mediatr, 
            IHttpContextAccessor httpContextAccessor)
        {
            _mediatr = mediatr;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId() => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string GetUserEmail() => _httpContextAccessor.HttpContext?.User?.FindFirstValue("emails");

        public string GetUserFullName()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return $"{user?.FindFirstValue(ClaimTypes.GivenName)} {user?.FindFirstValue(ClaimTypes.Surname)}";
        }

        public string GetUserAvatar() => null;

        public async Task<CurrentUserViewModel> GetCurrentUser(CancellationToken cancellationToken)
        {
            if (_currentUser != null)
            {
                return _currentUser;
            }

            try
            {
                return await _mediatr.Send(new GetCurrentUserQuery(), cancellationToken);
            }
            catch (NotFoundException)
            {
            }

            await _mediatr.Send(new UpsertCurrentUserCommand(), cancellationToken);

            return _currentUser = await _mediatr.Send(new GetCurrentUserQuery(), cancellationToken);
        }
    }
}
