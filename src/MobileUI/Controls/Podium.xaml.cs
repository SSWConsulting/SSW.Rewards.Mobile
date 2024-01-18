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

	private async Task LeaderChanged(LeaderViewModel leader)
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

		await RunAnimations(leader.Rank);
	}

	private string GetIcon(int rank)
	{
        return rank switch
		{
            1 => "👑",
            2 => "🥈",
			3 => "🥉",
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
                ProfilePic.WidthRequest = 130;
                ProfilePic.HeightRequest = 130;
				ProfilePic.CornerRadius = 65;
				ProfilePic.BorderWidth = 5;
                break;
            default:
                ProfilePic.WidthRequest = 70;
                ProfilePic.HeightRequest = 70;
				ProfilePic.CornerRadius = 35;
                ProfilePic.BorderWidth = 2;
                break;
        }
    }

	private async Task RunAnimations(int rank)
	{
		// set initial conditions

		ProfilePic.Opacity = 0;
		ProfilePic.Scale = 0;
		ProfilePic.TranslationY = 0;

		Icon.Opacity = 0;
		RankLabel.Opacity = 0;
		Name.Opacity = 0;
		Points.Opacity = 0;

        ParentLayout.IsVisible = true;

		// animate in

		if (rank == 1)
		{
			await Task.Delay(300);
		}
		else
		{
			await Task.Delay(1000);
		}

		await Task.WhenAll<bool>
		(
            ProfilePic.FadeTo(1, 500, Easing.SinIn),
			ProfilePic.ScaleTo(1, 500, Easing.SinIn)
		);

		var translationY = rank switch
		{
            1 => 30,
            _ => 90,
        };

		await ProfilePic.TranslateTo(0, translationY, 500, Easing.SinIn);

		Icon.TranslationY = translationY;
		RankLabel.TranslationY = translationY;
		Name.TranslationY = translationY;
		Points.TranslationY = translationY;

		if (rank != 1)
		{
			await Task.Delay(150);
		}

		await Task.WhenAll<bool>
		(
			Icon.FadeTo(1, 100, Easing.SinIn),
			RankLabel.FadeTo(1, 100, Easing.SinIn),
			Name.FadeTo(1, 100, Easing.SinIn),
			Points.FadeTo(1, 100, Easing.SinIn)
		);
    }
}