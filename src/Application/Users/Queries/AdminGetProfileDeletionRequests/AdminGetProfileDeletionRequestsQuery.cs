using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.AdminGetProfileDeletionRequests;


public class AdminGetProfileDeletionRequestsQuery : IRequest<ProfileDeletionRequestsVieWModel> { }

public class AdminProfileDeletionRequestsQueryHandler : IRequestHandler<AdminGetProfileDeletionRequestsQuery, ProfileDeletionRequestsVieWModel>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public AdminProfileDeletionRequestsQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ProfileDeletionRequestsVieWModel> Handle(AdminGetProfileDeletionRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _dbContext.OpenProfileDeletionRequests
            .ProjectTo<ProfileDeletionRequestDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new ProfileDeletionRequestsVieWModel
        {
            Requests = requests
        };
    }
}