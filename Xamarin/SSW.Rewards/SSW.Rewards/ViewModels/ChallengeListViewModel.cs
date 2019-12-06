using System;
using SSW.Rewards.Models;

namespace SSW.Rewards.ViewModels
{
    public class ChallengeListViewModel
    {
        public ChallengeListViewModel()
        {
        }

        public MyChallenge Challenge { get; set; }
        public bool IsHeader { get; set; }
        public bool IsRow { get; set; }
        public bool IsPointsHeader { get; set; }
        public string HeaderTitle { get; set; }
    }
}
