﻿using SSW.Rewards.Application.Common.Helpers;
using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.Application.Achievements.Command.PostAchievement;

public class CreateAchievementCommand : IRequest<AchievementAdminDto>
{
    public string Name { get; set; }
    public int Value { get; set; }
    public AchievementType Type { get; set; }

    public bool IsMultiscanEnabled { get; set; }
}

public class CreateAchievementCommandHandler : IRequestHandler<CreateAchievementCommand, AchievementAdminDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateAchievementCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AchievementAdminDto> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = new Achievement
        {
            Name = request.Name,
            Value = request.Value,
            Type = request.Type,
            IsMultiscanEnabled = request.IsMultiscanEnabled,
            Code = AchievementHelper.GenerateCode(),
        };

        _context.Achievements.Add(achievement);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AchievementAdminDto>(achievement);
    }
}
