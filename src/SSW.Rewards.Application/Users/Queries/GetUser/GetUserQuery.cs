using MediatR;
using SSW.Rewards.Application.Users.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Users.Queries.GetUser
{
    public class GetUserQuery : IRequest<UserViewModel>
    {
        public int Id { get; set; }
    }

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserViewModel>
    {
        private readonly IUserService _userService;

        public GetUserQueryHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<UserViewModel> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUser(request.Id, cancellationToken);
        }
    }
}