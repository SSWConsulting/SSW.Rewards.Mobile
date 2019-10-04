using System;
using System.Linq;

namespace SSW.Consulting.Application.Staff.Queries.GetStaffList
{
    public class StaffDto
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string[] Skills { get; set; } = Array.Empty<string>();
        public string Profile { get; set; }
        public Uri ProfilePhoto { get; set; }
        public string TwitterUsername { get; set; }
    }
}
