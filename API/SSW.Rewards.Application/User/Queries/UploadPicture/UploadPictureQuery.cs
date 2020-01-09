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
    public class UploadPictureQuery : IRequest<string>
    {
        public IFormFile File { get; set; }

        public class UpdateProfilePictureQueryHandler : IRequestHandler<UploadPictureQuery, string>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;
            private readonly IAvatarStorageProvider _storage;
            public ICurrentUserService _currentUserService { get; }


            public UpdateProfilePictureQueryHandler(
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


            public async Task<string> Handle(UploadPictureQuery request, CancellationToken cancellationToken)
            {
                var url = await _storage.UploadAvatar(request.File);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == _currentUserService.GetUserEmail());
                user.Avatar = url + ".png";
                _ = await _context.SaveChangesAsync(cancellationToken);

                return url;
            }
        
        }
    }
}
