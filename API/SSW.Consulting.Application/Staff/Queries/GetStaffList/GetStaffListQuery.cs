using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.Staff.Queries.GetStaffList
{
    public class GetStaffListQuery : IRequest<StaffListViewModel>
    {
        public sealed class Handler : IRequestHandler<GetStaffListQuery, StaffListViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWConsultingDbContext _dbContext;
            private readonly IProfileStorageProvider _storage;

            public Handler(
                IMapper mapper,
                ISSWConsultingDbContext dbContext,
                IProfileStorageProvider storage)
            {
                _mapper = mapper;
                _dbContext = dbContext;
                _storage = storage;
            }

            public async Task<StaffListViewModel> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
            {
                var staffDtos = await _dbContext
                    .StaffMembers
                    .Include(s => s.StaffMemberSkills)
                        .ThenInclude(sms => sms.Skill)
                    .ProjectTo<StaffDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                var staffDtosWithProfilePhotos = await Task.WhenAll(staffDtos.Select(staffMember => GetProfilePhoto(staffMember)));

                return new StaffListViewModel
                {
                    Staff = staffDtosWithProfilePhotos
                };
            }

            private async Task<StaffDto> GetProfilePhoto(StaffDto staffMember)
            {
                staffMember.ProfilePhoto = await _storage.GetProfileUri(staffMember.Name);
                return staffMember;
            }
        }
    }
}
