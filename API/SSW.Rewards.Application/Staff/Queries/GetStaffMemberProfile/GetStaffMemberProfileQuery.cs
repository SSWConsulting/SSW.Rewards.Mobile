using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffMemberProfile
{
    public class GetStaffMemberProfileQuery: IRequest<StaffMember>
    {
        public string Name { get; set; }
        public sealed class Handler : IRequestHandler<GetStaffMemberProfileQuery, StaffMember>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _dbContext;
            private readonly IProfileStorageProvider _storage;

            public Handler(
                IMapper mapper,
                ISSWRewardsDbContext dbContext,
                IProfileStorageProvider storage)
            {
                _mapper = mapper;
                _dbContext = dbContext;
                _storage = storage;
            }

            public async Task<StaffMember> Handle(GetStaffMemberProfileQuery request, CancellationToken cancellationToken)
            {
                var staffMember = await _dbContext
                    .StaffMembers
                    .Include(s => s.StaffMemberSkills)
                        .ThenInclude(sms => sms.Skill)
                    .ProjectTo<StaffMember>(_mapper.ConfigurationProvider)
                    .Where(member => member.Name == request.Name)
                    .FirstOrDefaultAsync();

                return staffMember;
            }

            private async Task<StaffMember> GetProfilePhoto(StaffMember staffMember)
            {
                staffMember.ProfilePhoto = await _storage.GetProfileUri(staffMember.Name);
                return staffMember;
            }
        }
    }
}
