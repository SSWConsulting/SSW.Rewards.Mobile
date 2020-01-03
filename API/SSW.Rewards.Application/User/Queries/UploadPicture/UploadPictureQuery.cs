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

            public UpdateProfilePictureQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context,
                IAvatarStorageProvider storage)
            {
                _mapper = mapper;
                _context = context;
                _storage = storage;
            }

            public async Task<string> Handle(UploadPictureQuery request, CancellationToken cancellationToken)
            {
                var id = await _storage.UploadAvatar(request.File);
                return id;
            }
        
        }
    }
}
