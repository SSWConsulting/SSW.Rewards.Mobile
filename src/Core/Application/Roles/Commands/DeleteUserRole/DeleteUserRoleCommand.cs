namespace SSW.Rewards.Application.Roles.Commands.DeleteUserRole;

public class DeleteUserRoleCommand : IRequest<Unit>
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}

public class DeleteUserRoleCommandHandler : IRequestHandler<DeleteUserRoleCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DeleteUserRoleCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(DeleteUserRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.UserRoles
            .Where(x => x.UserId == request.UserId && x.RoleId == request.RoleId)
            .SingleOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Unit.Value;
        }

        _context.UserRoles.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}