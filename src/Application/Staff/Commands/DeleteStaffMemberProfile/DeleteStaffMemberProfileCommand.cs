using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Staff.Commands.DeleteStaffMemberProfile;

public class DeleteStaffMemberProfileCommand : IRequest
{
    public int Id { get; set; }
}

public class DeleteStaffMemberProfileCommandHandler : IRequestHandler<DeleteStaffMemberProfileCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteStaffMemberProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteStaffMemberProfileCommand request, CancellationToken cancellationToken)
    {
        var staffMember = await _context.StaffMembers
            .TagWithContext()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(StaffMember), request.Id);

        // Soft delete the staff member.
        _context.StaffMembers.Remove(staffMember);

        await _context.SaveChangesAsync(cancellationToken);
    }
}