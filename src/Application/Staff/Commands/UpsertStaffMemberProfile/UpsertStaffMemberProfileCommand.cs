using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Helpers;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
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
    private readonly ICacheService _cacheService;

    public UpsertStaffMemberProfileCommandHandler(
        IMapper mapper,
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _mapper = mapper;
        _context = context;
        _cacheService = cacheService;
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
            // Check if email already exists
            var emailConflict = await _context.StaffMembers
                .AnyAsync(s => s.Email == request.Email, cancellationToken);

            if (emailConflict)
                throw new InvalidOperationException($"A staff member with email '{request.Email}' already exists.");

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
            Code = AchievementHelper.GenerateCode(),
            Type = AchievementType.Scanned
        };
        staffMemberEntity.StaffAchievement.Value = request.Points;

        var existingUser = await _context.Users
            .TagWithContext("GetUserByEmail")
            .Where(u => u.Email == request.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser == null)
        {
            var newUser = new Domain.Entities.User
            {
                Email = request.Email,
                FullName = request.Name,
                CreatedUtc = DateTime.UtcNow
            };

            if (!newUser.Email.ToLower().Contains("ssw.com.au"))
            {
                newUser.GenerateAchievement();
            }

            var roles = await _context.Roles
                .TagWithContext("GetUserAndStaffRoles")
                .Where(r => r.Name == "User" || r.Name == "Staff")
                .ToArrayAsync();

            foreach (var role in roles)
            {
                newUser.Roles.Add(new UserRole { Role = role });
            }

            await _context.Users.AddAsync(newUser);

            _cacheService.Remove(CacheTags.NewlyUserCreated);
        }

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
}
