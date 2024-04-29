using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.GetUserPendingRedemptions;

public class GetUserPendingRedemptionsQuery : IRequest<UserPendingRedemptionsViewModel>
{
    public int UserId { get; set; }
}

public class GetUserPendingRedemptionsQueryHandler : IRequestHandler<GetUserPendingRedemptionsQuery, UserPendingRedemptionsViewModel>
{
    private readonly IUserService _userService;

    public GetUserPendingRedemptionsQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserPendingRedemptionsViewModel> Handle(GetUserPendingRedemptionsQuery request, CancellationToken cancellationToken)
    {
        return await _userService.GetUserPendingRedemptions(request.UserId, cancellationToken);
    }
}