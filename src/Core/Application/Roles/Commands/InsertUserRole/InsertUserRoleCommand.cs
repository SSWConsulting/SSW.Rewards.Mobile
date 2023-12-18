namespace SSW.Rewards.Application.Roles.Commands.InsertUserRole;

public class InsertUserRoleCommand : IRequest<int>
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}

public class InsertUserRoleCommandHandler : IRequestHandler<InsertUserRoleCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public InsertUserRoleCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(InsertUserRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId
        };

        _context.UserRoles.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}