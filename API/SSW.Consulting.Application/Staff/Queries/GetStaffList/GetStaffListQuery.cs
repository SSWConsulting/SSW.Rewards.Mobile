//using AutoMapper;
//using AutoMapper.QueryableExtensions;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using SSW.Consulting.Application.Interfaces;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SSW.Consulting.Application.Staff.Queries.GetStaffList
//{
//    public class GetStaffListQuery : IRequest<StaffListViewModel>
//    {
//        public sealed class Handler : IRequestHandler<GetStaffListQuery, StaffListViewModel>
//        {
//            private readonly IMapper _mapper;
//            private readonly ISSWConsultingDbContext _dbContext;
//            private readonly IProfileStorageProvider _storage;

//            public Handler(
//                IMapper mapper,
//                ISSWConsultingDbContext dbContext,
//                IProfileStorageProvider storage)
//            {
//                _mapper = mapper;
//                _dbContext = dbContext;
//                _storage = storage;
//            }

//            public async Task<StaffListViewModel> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
//            {
//                var staff = await _dbContext
//                    .StaffMembers
//                    .ProjectTo<StaffDto>(_mapper.ConfigurationProvider)
//                    .ToListAsync(cancellationToken);

//                foreach (var staffMember in staff)
//                {

//                    //if (await _storage.Exists("", ""))
//                    //{
//                    //    var profilePhotoUrl = _storage.
//                    //}
//                }

//                var model = new StaffListViewModel
//                {
//                    Staff = staff
//                };

//                return model;
//            }
//        }
//    }
//}
