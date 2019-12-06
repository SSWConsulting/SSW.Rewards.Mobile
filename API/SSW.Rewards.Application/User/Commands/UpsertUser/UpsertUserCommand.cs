using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.User.Commands.UpsertUser
{
    public class UpsertUserCommand : IRequest<Unit>
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string Picture { get; set; }

        public class UpsertUserCommandHandler : IRequestHandler<UpsertUserCommand, Unit>
        {
            private readonly IMapper _mapper;
            private readonly ISSWConsultingDbContext _context;

            public UpsertUserCommandHandler(
                IMapper mapper,
                ISSWConsultingDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Unit> Handle(UpsertUserCommand request, CancellationToken cancellationToken)
            {
                var user = new Domain.Entities.User();
                if (request.Id == 0)
                {
                    _context.Users.Add(user);
                }
                else
                {
                    user = await _context
                        .Users
                        .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

                    if (user == null)
                    {
                        throw new NotFoundException(nameof(User), request.Id);
                    }
                }

                _mapper.Map(request, user);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
