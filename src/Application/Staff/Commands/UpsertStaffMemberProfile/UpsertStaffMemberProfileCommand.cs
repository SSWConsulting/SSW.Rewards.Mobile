using System.Text;
using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.Application.Staff.Commands.UpsertStaffMemberProfile;

public class UpsertStaffMemberProfileCommand : IRequest<StaffMemberDto>
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string Title { get; set; }

    public string Email { get; set; }

    public string Profile { get; set; }

    public string TwitterUsername { get; set; }
    public string GitHubUsername { get; set; }
    public string LinkedInUrl { get; set; }

    public Uri? ProfilePhoto { get; set; }

    public int Points { get; set; }

    public ICollection<StaffSkillDto> Skills { get; set; }
}

public class UpsertStaffMemberProfileCommandHandler : IRequestHandler<UpsertStaffMemberProfileCommand, StaffMemberDto>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public UpsertStaffMemberProfileCommandHandler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<StaffMemberDto> Handle(UpsertStaffMemberProfileCommand request, CancellationToken cancellationToken)
    {
        var staffMemberEntity = await _context.StaffMembers
            .Where(u => u.Id == request.Id)
            .Include(s => s.StaffMemberSkills).ThenInclude(sms => sms.Skill)
            .Include(s => s.StaffAchievement)
            .FirstOrDefaultAsync(cancellationToken);

        // Add if doesn't exist
        if (staffMemberEntity == null)
        {
            staffMemberEntity = new StaffMember();
            await _context.StaffMembers.AddAsync(staffMemberEntity, cancellationToken);
        }

        staffMemberEntity.Email = request.Email ??= string.Empty;
        staffMemberEntity.Name = request.Name ??= string.Empty;
        staffMemberEntity.Profile = request.Profile ??= string.Empty;
        staffMemberEntity.TwitterUsername = request.TwitterUsername ??= string.Empty;
        staffMemberEntity.GitHubUsername = request.GitHubUsername ??= string.Empty;
        staffMemberEntity.LinkedInUrl = request.LinkedInUrl ??= string.Empty;
        staffMemberEntity.Title = request.Title ??= string.Empty;

        await UpdateSkills(request, staffMemberEntity, cancellationToken);

        // Add staff achievement if it doesn't exist
        staffMemberEntity.StaffAchievement ??= new Achievement
        {
            Name = staffMemberEntity.Name,
            Code = GenerateCode(staffMemberEntity.Name),
            Type = AchievementType.Scanned
        };
        staffMemberEntity.StaffAchievement.Value = request.Points;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<StaffMemberDto>(staffMemberEntity);
    }

    private async Task UpdateSkills(UpsertStaffMemberProfileCommand request,
        StaffMember staffMemberEntity,
        CancellationToken cancellationToken)
    {

        if (request?.Skills == null)
            return;

        var staffSkills = staffMemberEntity.StaffMemberSkills;

        var skillsToRemove = staffSkills
            .Where(sms => !request.Skills.Any(rs => rs.Name == sms.Skill.Name))
            .ToList();

        var skillsToAdd = request.Skills
            .Where(sk => !staffSkills.Any(sms => sms.Skill.Name == sk.Name))
            .ToList();

        var skillsToUpdate = staffSkills
            .Where(sms => !skillsToAdd.Any(sk => sk.Name == sms.Skill.Name))
            .Where(sms => !skillsToRemove.Contains(sms))
            .Where(sms => sms.Level != request.Skills.First(sk => sk.Name == sms.Skill.Name).Level)
            .ToList();

        foreach (var skillToRemove in skillsToRemove)
        {
            _context.StaffMemberSkills.Remove(skillToRemove);
        }

        foreach (var skillToAdd in skillsToAdd)
        {
            var skill = await GetSkill(skillToAdd.Name, cancellationToken);

            staffMemberEntity.StaffMemberSkills.Add(new StaffMemberSkill
            {
                SkillId = skill.Id,
                Level = skillToAdd.Level
            });
        }
        foreach (var updateSkill in skillsToUpdate)
        {
            var staffSkill = request.Skills.FirstOrDefault(sms => sms.Name == updateSkill.Skill.Name);
            updateSkill.Level = staffSkill.Level;
        }

    }

    private async Task<Skill> GetSkill(
        string name,
        CancellationToken cancellationToken)
    {
        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);

        if (skill is null)
        {
            skill = new Skill
            {
                Name = name
            };
            await _context.Skills.AddAsync(skill, cancellationToken);
        }

        return skill;
    }

    private static string GenerateCode(string inputValue)
    {
        var codeData = Encoding.ASCII.GetBytes($"ach:{inputValue}");
        return Convert.ToBase64String(codeData);
    }
}
