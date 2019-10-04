using AutoMapper;
using SSW.Consulting.Domain.Entities;

namespace SSW.Consulting.Application.Staff.Queries.GetStaffList
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<StaffMember, StaffDto>();
        }
    }
}
