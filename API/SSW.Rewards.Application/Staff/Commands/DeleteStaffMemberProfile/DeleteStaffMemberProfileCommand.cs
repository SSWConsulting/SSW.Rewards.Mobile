using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Staff.Commands.DeleteStaffMemberProfile
{
    public class DeleteStaffMemberProfileCommand : IRequest<string>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public string TwitterUsername { get; set; }

        public class DeleteStaffMemberProfileCommandHandler : IRequestHandler<DeleteStaffMemberProfileCommand, string>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public DeleteStaffMemberProfileCommandHandler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<string> Handle(DeleteStaffMemberProfileCommand request, CancellationToken cancellationToken)
            {
                var staffMember = await _context.StaffMembers
                    .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

                if (staffMember == null)
                {
                    throw new NotFoundException(nameof(StaffMember), request.Name);
                }

                staffMember.IsDeleted = !staffMember.IsDeleted;
                await _context.SaveChangesAsync(cancellationToken);

                return staffMember.Id.ToString();
            }
        }
    }
}
