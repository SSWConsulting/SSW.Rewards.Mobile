using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.GetUsers;

public class GetUsersQuery : IRequest<UsersViewModel>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, UsersViewModel>
{
    private readonly IUserService _userService;

    public GetUsersQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UsersViewModel> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _userService.GetUsers(cancellationToken);
    }
}