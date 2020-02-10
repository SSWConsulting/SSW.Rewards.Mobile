using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
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
	            await using var ms = new MemoryStream();
				IFormFile file = request.File;
				await file.CopyToAsync(ms, cancellationToken);

				byte[] bytes = ms.ToArray();

				string filename = file.FileName;

				string url = await _storage.UploadProfilePic(bytes, filename);
				Domain.Entities.User user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == _currentUserService.GetUserEmail(), cancellationToken);
				user.Avatar = url;
				await _context.SaveChangesAsync(cancellationToken);

				return url;
			}
        }
    }
}
