using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Interfaces;
using SSW.Consulting.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Persistence
{
    public class SampleDataSeeder
    {
        private readonly ISSWConsultingDbContext _context;
        
        public SampleDataSeeder(ISSWConsultingDbContext context)
        {
            _context = context;
        }

        public async Task SeedAllAsync(CancellationToken cancellationToken)
        {
            if (await _context.StaffMembers.AnyAsync(cancellationToken))
            {
                return;
            }

            await SeedStaffAsync(cancellationToken);
        }

        private async Task SeedStaffAsync(CancellationToken cancellationToken)
        {
            await _context.StaffMembers.AddAsync(new Domain.Entities.StaffMember { Name = "Adam Cogan", Email = "adamcogan@ssw.com.au", Skills = new [] { "stuff", "things" } .Select(n => new Skill { Name = n }).ToList() }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}