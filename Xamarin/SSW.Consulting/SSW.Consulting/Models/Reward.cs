using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Models
{
    public class Reward
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public DateTimeOffset? AwardedAt { get; set; }
        public bool Awarded { get; set; }
    }
}
