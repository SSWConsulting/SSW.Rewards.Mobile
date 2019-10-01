using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Domain.Entities
{
    public class StaffMember
    {
        public StaffMember()
        {
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
    }
}


