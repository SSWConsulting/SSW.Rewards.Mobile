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

namespace SSW.Rewards.Application.Staff.Commands.UpsertStaffMemberProfile
{
    public class UpsertStaffMemberProfileCommand : IRequest<Unit>
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public string TwitterUsername { get; set; }

        public class UpsertStaffMemberProfileCommandHandler : IRequestHandler<UpsertStaffMemberProfileCommand, Unit>
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

            public async Task<Unit> Handle(UpsertStaffMemberProfileCommand request, CancellationToken cancellationToken)
            {
                var staffMember = new StaffMember();
                if (request.Name == "")
                {
                    _context.StaffMembers.Add(staffMember);
                }
                else
                {
                    staffMember = await _context.StaffMembers
                        .FirstOrDefaultAsync(u => u.Name == request.Name, cancellationToken);

                    if (staffMember == null)
                    {
                        throw new NotFoundException(nameof(StaffMember), request.Name);
                    }

                    if (request.Email != null)
                    {
                        staffMember.Email = request.Email;
                    }
                    else if(request.Name != null)
                    {
                        staffMember.Name = request.Name;
                    }
                    else if(request.Profile != null)
                    {
                        staffMember.Title = request.Title;
                    }
                    else if(request.TwitterUsername != null)
                    {
                        staffMember.TwitterUsername = request.TwitterUsername;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
