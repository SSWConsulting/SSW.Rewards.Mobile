using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffMemberProfile;

public class GetStaffMemberProfileQuery : IRequest<StaffMemberDto>
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
        IQueryable<StaffMemberDto> query = _dbContext.StaffMembers
            .IgnoreQueryFilters()
            .TagWithContext($"GetStaffMemberBy{(request.GetByEmail ? "Email" : "Id")}")
            .Include(s => s.StaffMemberSkills)
            .ThenInclude(sms => sms.Skill)
            .ProjectTo<StaffMemberDto>(_mapper.ConfigurationProvider);

        query = request.GetByEmail
            ? query.Where(member => member.Email == request.email)
            : query.Where(member => member.Id == request.Id);

        var staffMember = await query.FirstOrDefaultAsync(cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException(nameof(StaffMember), request.GetByEmail ? request.email : request.Id);
        }

        return staffMember;
    }
}
