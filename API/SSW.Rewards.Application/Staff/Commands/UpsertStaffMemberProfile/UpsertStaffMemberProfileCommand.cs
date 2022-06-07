using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Staff.Queries.GetStaffList;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Staff.Commands.UpsertStaffMemberProfile
{
    public class UpsertStaffMemberProfileCommand : IRequest<StaffDto>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Title { get; set; }

        public string Email { get; set; }

        public string Profile { get; set; }

        public string TwitterUsername { get; set; }
        public string GitHubUsername { get; set; }
        public string LinkedInUrl { get; set; }

        public Uri ProfilePhoto { get; set; }

        public int Rate { get; set; }

        public ICollection<StaffSkillDto> Skills { get; set; }
    }

    public class UpsertStaffMemberProfileCommandHandler : IRequestHandler<UpsertStaffMemberProfileCommand, StaffDto>
    {
        private readonly IMapper _mapper;
        private readonly ISSWRewardsDbContext _context;

        public UpsertStaffMemberProfileCommandHandler(
            IMapper mapper,
            ISSWRewardsDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<StaffDto> Handle(UpsertStaffMemberProfileCommand request, CancellationToken cancellationToken)
        {
            var staffMember = _mapper.Map<StaffMember>(request);
            var staffMemberEntity = await _context.StaffMembers
                .Where(u => u.Id == request.Id)
                .Include(s => s.StaffMemberSkills)
                .ThenInclude(sms => sms.Skill)
                .FirstOrDefaultAsync(cancellationToken);

            // Add if doesn't exist
            if (staffMemberEntity == null)
            {
                await _context.StaffMembers.AddAsync(staffMember, cancellationToken);
            }
            else // Update existing entity
            {
                staffMemberEntity.Email = request.Email ??= String.Empty;
                staffMemberEntity.Name = request.Name ??= String.Empty;
                staffMemberEntity.Profile = request.Profile ??= String.Empty;
                staffMemberEntity.TwitterUsername = request.TwitterUsername ??= String.Empty;
                staffMemberEntity.GitHubUsername = request.GitHubUsername ??= String.Empty;
                staffMemberEntity.LinkedInUrl = request.LinkedInUrl ??= String.Empty;
                staffMemberEntity.Title = request.Title ??= String.Empty;


                // Update staff skills
                if (request.Skills.Any())
                {
                    var staffSkills = await _context.StaffMemberSkills
                        .Where(sms => sms.StaffMemberId == staffMemberEntity.Id)
                        .ToListAsync(cancellationToken);

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
                        var skill = await _context.Skills
                            .FirstOrDefaultAsync(s => s.Name == skillToAdd.Name, cancellationToken);

                        _context.StaffMemberSkills.Add(new StaffMemberSkill
                        {
                            StaffMemberId = staffMemberEntity.Id,
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
            }

            // Add staff achievement if it doesn't exist
            staffMember.StaffAchievement ??= new Domain.Entities.Achievement
            {
                Name = staffMember.Name,
                Value = 200,
                Code = GenerateCode(staffMember.Name),
                Type = AchievementType.Scanned
            };

            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<StaffDto>(request);
        }

        private string GenerateCode(string inputValue)
        {
            var codeData = Encoding.ASCII.GetBytes(inputValue);
            return Convert.ToBase64String(codeData);
        }
    }
}
