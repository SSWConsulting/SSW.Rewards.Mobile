using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class Podium : ContentView
{
	public Podium()
	{
		InitializeComponent();
	}

	[AutoBindable(OnChanged = nameof(LeaderChanged))]
	private LeaderViewModel _leader;

	private void LeaderChanged(LeaderViewModel leader)
	{
		if (leader is null)
		{
			return;
		}

		ParentLayout.IsVisible = false;

		Icon.Text = GetIcon(leader.Rank);
        RankLabel.Text = leader.Rank.ToString();
		Name.Text = GetName(leader.Name);
		Points.Text = $"⭐ {leader.DisplayPoints:N0}";
		ProfilePic.ImageSource = leader.ProfilePic;

		Icon.FontSize = GetFontSize(leader.Rank);
		RankLabel.FontSize = GetFontSize(leader.Rank);
		Name.FontSize = GetFontSize(leader.Rank);
		Points.FontSize = GetFontSize(leader.Rank) - 3;

		SetAvatarSize(leader.Rank);

		ParentLayout.IsVisible = true;
	}

	private string GetIcon(int rank)
	{
        return rank switch
		{
            1 => "👑",
            2 => "🥈",
            _ => string.Empty,
        };
    }

	private string GetName(string name)
	{
		var names = name.Split(' ');
		if (names.Length > 1)
		{
            return $"{names[0]} {names[^1][0]}";
        }
        else
		{
            return name;
        }
	}

	private double GetFontSize(int rank)
	{
		return rank switch
		{
			1 => 24,
			_ => 18,
		};
	}

	private void SetAvatarSize(int rank)
	{
        switch (rank)
		{
            case 1:
                ProfilePic.WidthRequest = 120;
                ProfilePic.HeightRequest = 120;
				ProfilePic.CornerRadius = 60;
				ProfilePic.BorderWidth = 5;
                break;
            default:
                ProfilePic.WidthRequest = 60;
                ProfilePic.HeightRequest = 60;
				ProfilePic.CornerRadius = 30;
                ProfilePic.BorderWidth = 2;
                break;
        }
    }
}