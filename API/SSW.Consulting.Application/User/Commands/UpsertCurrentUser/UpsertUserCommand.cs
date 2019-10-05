using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Exceptions;
using SSW.Consulting.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.User.Commands.UpsertCurrentUser
{
    public class UpsertCurrentUserCommand : IRequest<Unit>
    {
        public class UpsertUserCommandHandler : IRequestHandler<UpsertCurrentUserCommand, Unit>
        {
            private readonly IMapper _mapper;
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWConsultingDbContext _context;

            public UpsertUserCommandHandler(
                IMapper mapper,
                ICurrentUserService currentUserService,
                ISSWConsultingDbContext context)
            {
                _mapper = mapper;
                _currentUserService = currentUserService;
                _context = context;
            }

            public async Task<Unit> Handle(UpsertCurrentUserCommand request, CancellationToken cancellationToken)
            {
                var user = await _context
                    .Users
                    .Where(u => u.Email == _currentUserService.GetUserEmail())
                    .FirstOrDefaultAsync(cancellationToken);

                if(user == null)
                {
                    user = new Domain.Entities.User();
                    await _context.Users.AddAsync(user, cancellationToken);
                }

                _mapper.Map(_currentUserService, user);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
