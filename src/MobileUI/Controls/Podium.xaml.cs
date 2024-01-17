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

		Icon.Text = GetIcon(leader.Rank);
		Name.Text = GetName(leader.Name);
		Points.Text = $"⭐ {leader.DisplayPoints:N0}";
		ProfilePic.ImageSource = leader.ProfilePic;
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
}