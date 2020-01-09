using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace SSW.Rewards.Application.User.Queries.GetUserRewards
{
    public class UploadAvatarQuery : IRequest<Domain.Entities.User>
    {
        public IFormFile File { get; set; }

        public class UpdateAvatarQueryHandler : IRequestHandler<UploadAvatarQuery, Domain.Entities.User>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;
            private readonly IAvatarStorageProvider _storage;
            public ICurrentUserService _currentUserService { get; }


            public UpdateAvatarQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context,
                IAvatarStorageProvider storage,
                ICurrentUserService currentUserService)
            {
                _mapper = mapper;
                _context = context;
                _storage = storage;
                _currentUserService = currentUserService;
            }


            public async Task<Domain.Entities.User> Handle(UploadAvatarQuery request, CancellationToken cancellationToken)
            {
                var url = await _storage.UploadAvatar(request.File);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == _currentUserService.GetUserEmail());
                user.Avatar = url + ".png";
                _ = await _context.SaveChangesAsync(cancellationToken);

                return user;
            }
        
        }
    }
}
