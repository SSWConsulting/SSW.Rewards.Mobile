using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.System.Commands.Common;
using SSW.Rewards.Domain.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Users.Commands.UploadProfilePic
{
    public class UploadProfilePicCommand : IRequest<ProfilePicResponseDto>
    {
        public IFormFile File { get; set; }
    }

    public class UploadProfilePicHandler : IRequestHandler<UploadProfilePicCommand, ProfilePicResponseDto>
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

        public async Task<ProfilePicResponseDto> Handle(UploadProfilePicCommand request, CancellationToken cancellationToken)
        {
            var response = new ProfilePicResponseDto();

            await using var ms = new MemoryStream();

            IFormFile file = request.File;
            
            await file.CopyToAsync(ms, cancellationToken);

            byte[] bytes = ms.ToArray();

            string filename = file.FileName;

            string url = await _storage.UploadProfilePic(bytes, filename);
            
            var user = await _context.Users
                .Where(u => u.Email.ToLower() == _currentUserService.GetUserEmail())
                .Include(u => u.UserAchievements)
                    .ThenInclude(ua => ua.Achievement)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
            
            user.Avatar = url;

            var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.ProfilePic, cancellationToken);

            if (!user.UserAchievements.Any(a => a.Achievement == achievement))
            {
                user.UserAchievements.Add(new UserAchievement
                {
                    Achievement = achievement
                });

                response.AchievementAwarded = true;
            }

            response.PicUrl = url;
            
            await _context.SaveChangesAsync(cancellationToken);

            return response;
        }
    }
}
