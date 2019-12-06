using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards.Models
{
    public class LeaderSummary
    {
        public int id { get; set; }
        public string ProfilePic { get; set; }
        public string Name { get; set; }
        public int BaseScore { get; set; }
        public int BonusScore { get; set; }
        public int Rank { get; set; }

    }
}
