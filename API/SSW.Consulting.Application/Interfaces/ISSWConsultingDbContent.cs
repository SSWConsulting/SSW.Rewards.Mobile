using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Application.Interfaces
{
    public interface ISSWConsultingDbContent
    {
        DbSet<StaffMember> StaffMembers { get; set; }
    }
}
