using Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.GetUser;

public class GetUserQuery : IRequest<UserProfileDto>
{
    public int Id { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserProfileDto>
{
    private readonly IUserService _userService;

    public GetUserQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserProfileDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return await _userService.GetUser(request.Id, cancellationToken);
    }
}