using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SSW.Consulting.Domain.Entities
{
    public class StaffMember
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public List<Skill> Skills { get; set; } = new List<Skill>();
    }

    public class Skill
    {
        public string Name { get; set; }
    }
}


