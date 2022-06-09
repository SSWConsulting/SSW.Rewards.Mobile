using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class LeaderViewModel : LeaderboardUserDto
    {        
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

        public int DisplayPoints { get; set; }

        public LeaderViewModel(LeaderboardUserDto dto, bool isMe)
        {
            Rank = dto.Rank;

            UserId = dto.UserId;

            Name = dto.Name;

            ProfilePic = dto.ProfilePic;

            TotalPoints = dto.TotalPoints;

            PointsThisMonth = dto.PointsThisMonth;

            PointsThisYear = dto.PointsThisYear;

            IsMe = isMe;
        }
    }
}
