using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Interfaces;
using SSW.Consulting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Persistence
{
    public class SampleDataSeeder
    {
        private readonly ISSWConsultingDbContext _context;
        private Dictionary<string, Skill> _skills;

        public SampleDataSeeder(ISSWConsultingDbContext context)
        {
            _context = context;
        }

        public async Task SeedAllAsync(CancellationToken cancellationToken)
        {
            await SeedSkillsAsync(cancellationToken);
            await SeedStaffMembers(cancellationToken);
        }

        private async Task SeedSkillsAsync(CancellationToken cancellationToken)
        {
            if (!await _context.Skills.AnyAsync(cancellationToken))
            {

                var skills = new[] {
                    "Angular",
                    "React",
                    "SharePoint",
                    "Scrum",
                    "PowerBI",
                    "NETCore",
                    "Node",
                    "DevOps",
                    "iOS",
                    "Dancing",
                    "Smoking",
                    "Beer"
                }.Select(s => new Skill { Name = s });

                await _context.Skills.AddRangeAsync(skills, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            _skills = await _context.Skills.ToDictionaryAsync(s => FormatSkillName(s.Name), s => s, cancellationToken);
        }

        private async Task SeedStaffMembers(CancellationToken cancellationToken)
        {
            if (await _context.StaffMembers.AnyAsync(cancellationToken))
            {
                return;
            }

            var staff = new[] {
                new StaffMember { Name = "Adam Cogan", Title = "Chief Architect", Email = "adamcogan@ssw.com.au", TwitterUsername = "adamcogan", StaffMemberSkills = GetSkills("PowerBI", "Angular", "NETCore", "Scrum"), Profile = "This is profile text" },
            };

            await _context.StaffMembers.AddRangeAsync(staff, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private ICollection<StaffMemberSkill> GetSkills(params string[] skills)
        {
            return skills
                .Select(s =>
                {
                    if (_skills.TryGetValue(FormatSkillName(s), out var skill))
                    {
                        return skill;
                    }
                    return null;
                })
                .Where(skill => skill != null)
                .Select(skill => new StaffMemberSkill { SkillId = skill.Id })
                .ToList();
        }

        private static string FormatSkillName(string name) => name.ToLower().Replace(" ", string.Empty);
    }
}