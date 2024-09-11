using SSW.Rewards.Shared.DTOs.Users;

namespace Microsoft.Extensions.DependencyInjection.Users.Queries.GetSocialMediaId;

public class GetSocialMediaQuery : IRequest<UserSocialMediaDto>
{
    public int UserId { get; set; }
}

public class GetSocialMediaQueryHandler(IApplicationDbContext context) : IRequestHandler<GetSocialMediaQuery, UserSocialMediaDto>
{
    public async Task<UserSocialMediaDto> Handle(GetSocialMediaQuery request, CancellationToken cancellationToken)
    {
        var socialMedia = await context.UserSocialMediaIds
            .Where(x => x.UserId == request.UserId)
            .OrderByDescending(x => x.CreatedUtc)
            .Select(x => new UserSocialMediaIdDto { SocialMediaUserId = x.SocialMediaUserId })
            .ToListAsync(cancellationToken: cancellationToken);

        return new UserSocialMediaDto { SocialMedia = socialMedia };
    }
}