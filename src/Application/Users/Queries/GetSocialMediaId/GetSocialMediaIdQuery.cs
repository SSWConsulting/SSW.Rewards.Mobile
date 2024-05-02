using SSW.Rewards.Shared.DTOs.Users;

namespace Microsoft.Extensions.DependencyInjection.Users.Queries.GetSocialMediaId;

public class GetSocialMediaIdQuery : IRequest<UserSocialMediaIdDto>
{
    public int UserId { get; set; }
    public int SocialMediaPlatformId { get; set; }
}

public class GetSocialMediaIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetSocialMediaIdQuery, UserSocialMediaIdDto>
{
    public async Task<UserSocialMediaIdDto> Handle(GetSocialMediaIdQuery request, CancellationToken cancellationToken)
    {
        var socialMediaId = await context.UserSocialMediaIds
            .Where(x => x.UserId == request.UserId && x.SocialMediaPlatformId == request.SocialMediaPlatformId)
            .OrderByDescending(x => x.CreatedUtc)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return new UserSocialMediaIdDto { SocialMediaUserId = socialMediaId?.SocialMediaUserId };
    }
}