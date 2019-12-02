using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Models
{
    public class Achievement
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool Complete { get; set; }
        public DateTime? AwardedAt { get; set; }
    }
}
