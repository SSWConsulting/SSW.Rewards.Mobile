using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Staff.Queries.GetStaffList;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Uri ProfilePhoto { get; set; }

        public int Rate { get; set; }

        public List<string> Skills { get; set; }        
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
                .Include(s => s.StaffMemberSkills)
                .ThenInclude(sms => sms.Skill)
                .FirstOrDefaultAsync(u => u.Name == request.Name, cancellationToken);

            // Add if doesn't exist
            if (staffMemberEntity == null)
            {
                _context.StaffMembers.Add(staffMember);
            }
            else // Update existing entity
            {
                var staffMember = _mapper.Map<StaffMember>(request);
                var staffMemberEntity = await _context.StaffMembers
                    .Include(s => s.StaffMemberSkills)
                    .ThenInclude(sms => sms.Skill)
                    .FirstOrDefaultAsync(u => u.Id == staffMember.Id, cancellationToken);

                // Add if doesn't exist
                if (staffMemberEntity == null)
                {
                    _context.StaffMembers.Add(staffMember);
                }
                else // Update existing entity
                {
                    staffMemberEntity.Email = request.Email;
                    staffMemberEntity.Name = request.Name;
                    staffMemberEntity.Profile = request.Profile;
                    staffMemberEntity.TwitterUsername = request.TwitterUsername;
                    staffMemberEntity.Title = request.Title;

                // check for skills
                var skills = request.Skills;

                // changes have been made to members' skills
                if (staffMemberEntity.StaffMemberSkills.Count() != skills.Count())
                {
                    var newSkills = request.Skills.Where(x => !staffMemberEntity.StaffMemberSkills.Select(x => x.Skill.Name).Contains(x)).ToList();

                    if (staffMemberEntity.StaffMemberSkills.Count() < request.Skills.Count())
                    {
                        // assign the new skills to a member
                        foreach (var skill in newSkills)
                        {
                            var skillEntity = await _context.Skills.FirstOrDefaultAsync(x => x.Name == skill);
                            staffMemberEntity.StaffMemberSkills.Add(new StaffMemberSkill { Skill = skillEntity });
                        }
                    }
                    else
                    {
                        var deletedSkills = staffMemberEntity.StaffMemberSkills.Where(x => !request.Skills.Contains(x.Skill.Name)).ToList();
                        foreach (var deletedSkill in deletedSkills)
                        {
                            staffMemberEntity.StaffMemberSkills.Remove(deletedSkill);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<StaffDto>(request);
        }
    }
}
