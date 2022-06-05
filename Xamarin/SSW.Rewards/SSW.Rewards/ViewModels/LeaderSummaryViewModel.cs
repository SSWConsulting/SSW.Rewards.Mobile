using SSW.Rewards.Models;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class LeaderSummaryViewModel : BaseViewModel
    {
        public int Rank{ get { return _summary.Rank; } }
        public string ProfilePic { get { return _summary.ProfilePic; } }
        public string Name { get { return _summary.Name; } }
        public int Score { get { return _summary.BaseScore; } }
        public int Id { get { return _summary.id; } }
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

        public bool IsLeader
        {
            get
            {
                return Rank == 1;
            }
        }

        private bool _isMe;

        public FontAttributes fontAttribute
        {
            get
            {
                if (_isMe)
                    return FontAttributes.Bold;
                else
                    return FontAttributes.None;
            }
        }

        private LeaderSummary _summary;

        public LeaderSummaryViewModel(LeaderSummary summary)
        {
            _summary = summary;
        }
    }
}
