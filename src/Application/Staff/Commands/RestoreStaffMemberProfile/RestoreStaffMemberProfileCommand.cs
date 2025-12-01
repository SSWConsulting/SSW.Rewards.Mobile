using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Staff.Commands.RestoreStaffMemberProfile;

public class RestoreStaffMemberProfileCommand : IRequest
{
    public int Id { get; set; }
}

public class RestoreStaffMemberProfileCommandHandler : IRequestHandler<RestoreStaffMemberProfileCommand>
{
    private readonly IApplicationDbContext _context;

    public RestoreStaffMemberProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RestoreStaffMemberProfileCommand request, CancellationToken cancellationToken)
    {
        var staffMember = await _context.StaffMembers
            .IgnoreQueryFilters()
            .TagWithContext()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(StaffMember), request.Id);

        // Restore the staff member by clearing the soft delete fields.
        staffMember.DeletedUtc = null;
        staffMember.DeletedBy = null;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
