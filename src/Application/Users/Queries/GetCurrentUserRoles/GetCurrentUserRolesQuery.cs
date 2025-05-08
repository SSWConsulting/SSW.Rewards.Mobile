namespace SSW.Rewards.Application.Users.Queries.GetCurrentUserRoles;

public class GetCurrentUserRolesQuery : IRequest<string[]>
{

}

public class GetCurrentUserRolesQueryHandler : IRequestHandler<GetCurrentUserRolesQuery, string[]>
{
    private readonly IUserService _userService;

    public GetCurrentUserRolesQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<string[]> Handle(GetCurrentUserRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _userService.GetCurrentUserRoles(cancellationToken);

        return roles?.Select(r => r.Name ?? "").ToArray() ?? Array.Empty<string>();
    }
}
