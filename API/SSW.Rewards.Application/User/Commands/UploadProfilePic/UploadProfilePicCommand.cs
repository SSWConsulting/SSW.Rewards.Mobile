using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.User.Commands.UploadProfilePic
{
    public class UploadProfilePicCommand : IRequest<string>
    {
        public IFormFile File { get; set; }

        public class UploadProfilePicHandler : IRequestHandler<UploadProfilePicCommand, string>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;
            private readonly IProfilePicStorageProvider _storage;
            public ICurrentUserService _currentUserService { get; }

            public UploadProfilePicHandler(ICurrentUserService currentUserService,
                ISSWRewardsDbContext sSWRewardsDbContext,
                IProfilePicStorageProvider profilePicStorageProvider,
                IMapper mapper)
            {
                _currentUserService = currentUserService;
                _mapper = mapper;
                _storage = profilePicStorageProvider;
                _context = sSWRewardsDbContext;
            }

            public async Task<string> Handle(UploadProfilePicCommand request, CancellationToken cancellationToken)
            {
                var url = await _storage.UploadProfilePic(request.File);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == _currentUserService.GetUserEmail());
                user.Avatar = url;
                await _context.SaveChangesAsync(cancellationToken);

                return url;
            }
        }
    }
}
