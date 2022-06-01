using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffList
{
    public class GetStaffListQuery : IRequest<StaffListViewModel>
    {
        public sealed class Handler : IRequestHandler<GetStaffListQuery, StaffListViewModel>
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

            public async Task<StaffListViewModel> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
            {
                var staffDtos = await _dbContext.StaffMembers
                    .ProjectTo<StaffDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return new StaffListViewModel
                {
                    Staff = staffDtos
                };
            }
        }
    }
}
