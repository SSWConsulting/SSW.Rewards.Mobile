using AutoMapper;
using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Staff.Queries.GetStaffList;
using SSW.Rewards.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Staff.Commands.AddStaffMemberProfile
{
    public class AddStaffMemberProfileCommand : IRequest<StaffDto>
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public string TwitterUsername { get; set; }
        public bool IsExternal { get; set; }

        public class AddAchievementCommandHandler : IRequestHandler<AddStaffMemberProfileCommand, StaffDto>
        {
            private readonly ISSWRewardsDbContext _context;
            private readonly IMapper _mapper;

            public AddAchievementCommandHandler(ISSWRewardsDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<StaffDto> Handle(AddStaffMemberProfileCommand request, CancellationToken cancellationToken)
            {
                var staffMember = _mapper.Map<StaffMember>(request);

                await _context.StaffMembers.AddAsync(staffMember, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return null;
            }
        }
    }
}
