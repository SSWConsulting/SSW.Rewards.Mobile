using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Exceptions;
using SSW.Consulting.Application.Common.Interfaces;
using SSW.Consulting.Application.User.Queries.GetUser;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.User.Queries.GetCurrentUserQuery
{
    public class GetCurrentUserQuery : IRequest<UserViewModel>
    {
        public string Email { get; set; }

        public class GetUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWConsultingDbContext _context;

            public GetUserQueryHandler(
                IMapper mapper,
                ICurrentUserService currentUserService,
                ISSWConsultingDbContext context)
            {
                _mapper = mapper;
                _currentUserService = currentUserService;
                _context = context;
            }

            public async Task<UserViewModel> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .Where(u => u.Email == _currentUserService.GetUserEmail())
                    .ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.Email);
                }

                return user;
            }
        }
    }
}
