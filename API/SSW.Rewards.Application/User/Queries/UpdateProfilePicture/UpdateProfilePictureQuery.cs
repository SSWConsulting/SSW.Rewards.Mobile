using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.User.Queries.GetUserRewards
{
    public class UpdateProfilePictureQuery : IRequest<Domain.Entities.User>
    {
        public string Email { get; set; }
        public string Url { get; set; }

        public class UpdateProfilePictureQueryHandler : IRequestHandler<UpdateProfilePictureQuery, Domain.Entities.User>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public UpdateProfilePictureQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Domain.Entities.User> Handle(UpdateProfilePictureQuery request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email);
                user.Avatar = request.Url;
                _ = await _context.SaveChangesAsync(cancellationToken);
                return user;
            }
        }
    }
}
