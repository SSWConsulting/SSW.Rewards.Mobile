namespace SSW.Rewards.Mobile.Controls;

public partial class TabHeader : ContentView
{
    public TabHeader()
    {
        InitializeComponent();

        MonthUnderline.HeightRequest = 0;
        YearUnderline.HeightRequest = 5;
        AllUnderline.HeightRequest = 5;
    }

    public event EventHandler<FilterChangedEventArgs> FilterChanged;

    private void Radio_Tapped(object sender, TappedEventArgs e)
    {
        var lblSender = (Label)sender;

        var range = FilterRange.AllTime;

        switch (lblSender.Text)
        {
            case "This Month":
                range = FilterRange.Month;
                MonthUnderline.HeightRequest = 5;
                YearUnderline.HeightRequest = 0;
                AllUnderline.HeightRequest = 0;
                break;
            case "This Year":
                range = FilterRange.Year;
                MonthUnderline.HeightRequest = 0;
                YearUnderline.HeightRequest = 5;
                AllUnderline.HeightRequest = 0;
                break;
            case "All Time":
            default:
                range = FilterRange.AllTime;
                MonthUnderline.HeightRequest = 0;
                YearUnderline.HeightRequest = 0;
                AllUnderline.HeightRequest = 5;
                break;
        }

        FilterChanged.Invoke(this, new FilterChangedEventArgs { FilterRange = range });
    }
}

public enum FilterRange
{
	Month,
	Year,
	AllTime
}

public class FilterChangedEventArgs : EventArgs
{
	public FilterRange FilterRange;
}