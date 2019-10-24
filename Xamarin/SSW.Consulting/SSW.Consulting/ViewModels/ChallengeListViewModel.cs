using System;
using SSW.Consulting.Models;

namespace SSW.Consulting.ViewModels
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
