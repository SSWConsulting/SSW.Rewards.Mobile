using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Staff.Commands.DeleteStaffProfile;
public class DeleteStaffProfile : IRequest<Unit>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string Email { get; set; }
    public string Profile { get; set; }
    public string TwitterUsername { get; set; }

    public DeleteStaffProfile(int id, string name, string title, string email, string profile, string twitterUsername)
    {
        Id              = id;
        Name            = name;
        Title           = title;
        Email           = email;
        Profile         = profile;
        TwitterUsername = twitterUsername;
    }
}

public class DeleteStaffProfileHandler : IRequestHandler<DeleteStaffProfile, Unit>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public DeleteStaffProfileHandler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _mapper     = mapper;
        _context    = context;
    }

    public async Task<Unit> Handle(DeleteStaffProfile request, CancellationToken cancellationToken)
    {
        var staffMember = await _context.StaffMembers
                                        .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException(nameof(StaffMember), request.Name);
        }

        staffMember.IsDeleted = !staffMember.IsDeleted;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}