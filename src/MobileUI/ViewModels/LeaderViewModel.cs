using System.Net.Mail;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Mobile.ViewModels;

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

    public bool IsStaff
    {
        get
        {
            try
            {
                var emailAddress = new MailAddress(Email);
                return emailAddress.Host == "ssw.com.au";
            }
            catch (FormatException)
            {
                return false;
            }
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
    
    public string DisplayPointsShort
    {
        get
        {
            if (DisplayPoints >= 1000)
            {
                return (DisplayPoints / 1000).ToString("0.#") + "k";
            }
            
            return DisplayPoints.ToString("#,0");
        }
    }

    public LeaderViewModel(LeaderboardUserDto dto, bool isMe)
    {
        Rank = dto.Rank;

        UserId = dto.UserId;

        Name = dto.Name;

        ProfilePic = dto.ProfilePic;

        TotalPoints = dto.TotalPoints;

        PointsThisMonth = dto.PointsThisMonth;

        PointsThisYear = dto.PointsThisYear;

        Balance = dto.Balance;

        IsMe = isMe;

        Email = dto.Email;
    }
}
