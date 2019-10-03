using System;
using System.Collections.Generic;
using System.Text;
using SSW.Consulting.Models;
using SSW.Consulting.Services;

namespace SSW.Consulting.ViewModels
{
    public class LeaderSummaryViewModel : BaseViewModel
    {
        public int Rank{ get { return _summary.Rank; } }
        public string ProfilePic { get { return _summary.ProfilePic; } }
        public string Name { get { return _summary.Name; } }
        public int BaseScore { get { return _summary.BaseScore; } }
        public int BonusScore { get { return _summary.BonusScore; } }
        public bool IsMe
        {
            get
            {
                return _isMe;
            }
            set
            {
                _isMe = value;
            }
        }

        private bool _isMe;

        private LeaderSummary _summary;

        public LeaderSummaryViewModel(LeaderSummary summary)
        {
            _summary = summary;
        }
    }
}
