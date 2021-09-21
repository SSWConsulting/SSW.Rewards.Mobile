using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Staff.Queries.GetStaffList;
using SSW.Rewards.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Staff.Commands.UpsertStaffMemberProfile
{
    public class UpsertStaffMemberProfileCommand : IRequest<StaffDto>
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public string TwitterUsername { get; set; }
        public Uri ProfilePhoto { get; set; }

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
                        .FirstOrDefaultAsync(u => u.Name == request.Name, cancellationToken);

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
                }

                await _context.SaveChangesAsync(cancellationToken);

                return _mapper.Map<StaffDto>(request);
            }
        }
    }
}
