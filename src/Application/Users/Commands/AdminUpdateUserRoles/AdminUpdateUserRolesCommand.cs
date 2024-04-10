using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Commands.RegisterUser;

public class AdminUpdateUserRolesCommand : IRequest
{
    public UserDto User { get; set; } = new();
}

public class AdminUpdateUserRolesCommandHandler : IRequestHandler<AdminUpdateUserRolesCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public AdminUpdateUserRolesCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AdminUpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefault(x => x.Id == request.User.Id);

        if (user != null)
        {
            var rolesToRemove = user.Roles
                .Where(ur => !request.User.Roles.Any(r => r.Name == ur.Role.Name))
                .ToList();

            var rolesToAdd = request.User.Roles
                .Where(r => !user.Roles.Any(ur => ur.Role.Name == r.Name))
                .ToList();

            foreach (var roleToRemove in rolesToRemove)
            {
                user.Roles.Remove(roleToRemove);
            }

            foreach (var roleToAdd in rolesToAdd)
            {
                user.Roles.Add(new UserRole { RoleId = roleToAdd.Id, UserId = user.Id });
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
