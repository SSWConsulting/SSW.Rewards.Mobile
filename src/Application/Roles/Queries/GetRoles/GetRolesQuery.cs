using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Roles;

namespace SSW.Rewards.Application.Roles.Queries.GetRoles;

public class GetRolesQuery : IRequest<IEnumerable<RoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _dbContext;

    public GetRolesQueryHandler(IMapper mapper, IApplicationDbContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Roles.ProjectTo<RoleDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
    }
}