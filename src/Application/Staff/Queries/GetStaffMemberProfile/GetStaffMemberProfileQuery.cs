using AutoMapper.QueryableExtensions;
using Shared.DTOs.Staff;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffMemberProfile;

public class GetStaffMemberProfileQuery: IRequest<StaffMemberDto>
{
    public int Id { get; set; }

    public string email { get; set; }

    public bool GetByEmail { get; set; } = false;
}

public sealed class Handler : IRequestHandler<GetStaffMemberProfileQuery, StaffMemberDto>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _dbContext;
    private readonly IProfileStorageProvider _storage;

    public Handler(
        IMapper mapper,
        IApplicationDbContext dbContext,
        IProfileStorageProvider storage)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _storage = storage;
    }

    public async Task<StaffMemberDto> Handle(GetStaffMemberProfileQuery request, CancellationToken cancellationToken)
    {
        var staffMember = await _dbContext.StaffMembers
            .Include(s => s.StaffMemberSkills)
            .ThenInclude(sms => sms.Skill)
            .ProjectTo<StaffMemberDto>(_mapper.ConfigurationProvider)
            .Where(member => member.Id == request.Id)
            .FirstOrDefaultAsync();

        return staffMember;
    }
}
