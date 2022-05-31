using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementList;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using SSW.Rewards.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Achievements.Command.PostAchievement
{
    public class CreateAchievementCommand : IRequest<AchievementAdminViewModel>
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public class CreateAchievementCommandHandler : IRequestHandler<CreateAchievementCommand, AchievementAdminViewModel>
        {
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWRewardsDbContext _context;
            private readonly IMapper _mapper;

            public CreateAchievementCommandHandler(
                ICurrentUserService currentUserService,
                ISSWRewardsDbContext context,
                IMapper mapper)
            {
                _currentUserService = currentUserService;
                _context = context;
                _mapper = mapper;
            }

            public async Task<AchievementAdminViewModel> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
            {
                var existingAchievements = await _context.Achievements.ToListAsync(cancellationToken);

                var achievement = existingAchievements
                    .FirstOrDefault(a => a.Name.Equals(request.Name, StringComparison.InvariantCulture))
                    ?? new Domain.Entities.Achievement();

                achievement.Name = request.Name;
                var codeData = Encoding.ASCII.GetBytes(request.Name);
                achievement.Code = Convert.ToBase64String(codeData);
                achievement.Value = request.Value;
                achievement.Type = AchievementType.Attended;

                if (achievement.Id == 0)
                {
                    _context.Achievements.Add(achievement);
                    await _context.SaveChangesAsync(cancellationToken);
                    return new AchievementAdminViewModel()
                    {
                        Code = achievement.Code,
                        Name = achievement.Name,
                        Value = achievement.Value,
                        Type = achievement.Type,
                    };

                }

                return null;
            }
        }
    }
}
