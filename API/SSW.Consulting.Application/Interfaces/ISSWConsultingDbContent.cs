using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.Interfaces
{
    public interface ISSWConsultingDbContext
    {
        DbSet<StaffMember> StaffMembers { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
