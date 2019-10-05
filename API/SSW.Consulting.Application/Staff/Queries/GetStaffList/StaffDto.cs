using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.Consulting.Application.Staff.Queries.GetStaffList
{
    public class StaffDto
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public Uri ProfilePhoto { get; set; }
        public string TwitterUsername { get; set; }
        public IEnumerable<string> Skills { get; set; } = Enumerable.Empty<string>();
    }
}
