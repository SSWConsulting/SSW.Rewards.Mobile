using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.User.Commands.UploadProfilePic
{
    public class UploadProfilePicCommand : IRequest<UploadProfilePicResult>
    {
        public IFormFile File { get; set; }

        public class UploadProfilePicHandler : IRequestHandler<UploadProfilePicCommand, UploadProfilePicResult>
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

            public async Task<UploadProfilePicResult> Handle(UploadProfilePicCommand request, CancellationToken cancellationToken)
            {
                
            }
        }
    }
}
