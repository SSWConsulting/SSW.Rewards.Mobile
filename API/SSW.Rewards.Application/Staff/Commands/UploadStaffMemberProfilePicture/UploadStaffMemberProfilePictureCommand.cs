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

namespace SSW.Rewards.Application.Staff.Commands.UploadStaffMemberProfilePicture
{
    public class UploadStaffMemberProfilePictureCommand : IRequest<string>
    {
        public int Id { get; set; }
        public IFormFile File { get; set; }

        public class UploadProfilePicHandler : IRequestHandler<UploadStaffMemberProfilePictureCommand, string>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;
            private readonly IProfileStorageProvider _storage;
            public ICurrentUserService _currentUserService { get; }

            public UploadProfilePicHandler(ICurrentUserService currentUserService,
                ISSWRewardsDbContext sSWRewardsDbContext,
                IProfileStorageProvider profileStorageProvider,
                IMapper mapper)
            {
                _currentUserService = currentUserService;
                _mapper = mapper;
                _storage = profileStorageProvider;
                _context = sSWRewardsDbContext;
            }

            public async Task<string> Handle(UploadStaffMemberProfilePictureCommand request, CancellationToken cancellationToken)
            {
                await using var ms = new MemoryStream();
                IFormFile file = request.File;
                await file.CopyToAsync(ms, cancellationToken);

                byte[] bytes = ms.ToArray();

                string filename = file.FileName;

                var imgUrl = await _storage.UploadProfilePicture(bytes, filename);

                var staffMember = await _context.StaffMembers.FirstOrDefaultAsync(x => x.Id == request.Id);
                staffMember.ProfilePhoto = imgUrl;
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);

                }
                catch (Exception ex)
                {
                    
                }
                return imgUrl;
            }
        }
    }
}
