using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Staff.Commands.DeleteStaffMemberProfile;

public class DeleteStaffMemberProfileCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

public class DeleteStaffMemberProfileCommandHandler : IRequestHandler<DeleteStaffMemberProfileCommand, Unit>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public DeleteStaffMemberProfileCommandHandler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<Unit> Handle(DeleteStaffMemberProfileCommand request, CancellationToken cancellationToken)
    {
        var staffMember = await _context.StaffMembers
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException(nameof(StaffMember), request.Id);
        }

        staffMember.IsDeleted = !staffMember.IsDeleted;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}