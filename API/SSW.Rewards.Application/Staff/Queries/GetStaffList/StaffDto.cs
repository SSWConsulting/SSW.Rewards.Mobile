using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffList
{
    public class StaffDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public string ProfilePhoto { get; set; }
        public string TwitterUsername { get; set; }
        public bool IsExternal { get; set; }
        public IEnumerable<string> Skills { get; set; } = Enumerable.Empty<string>();
    }
}
