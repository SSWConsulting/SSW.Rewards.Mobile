namespace SSW.Rewards.Mobile.Controls;

public partial class TabHeader : ContentView
{
    private Color _selectedTextColor;
    private Color _unselectedTextColor;
    private int _underlineThickness = 2;

    public TabHeader()
    {
        InitializeComponent();

        MonthUnderline.HeightRequest = _underlineThickness;
        YearUnderline.HeightRequest = 0;
        AllUnderline.HeightRequest = 0;

        var dict = App.Current.Resources.MergedDictionaries.First();

        _selectedTextColor = (Color)dict["primary"];
        _unselectedTextColor = (Color)dict["MutedText"];

        MonthRadio.TextColor = _selectedTextColor;
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
                MonthUnderline.HeightRequest = _underlineThickness;
                YearUnderline.HeightRequest = 0;
                AllUnderline.HeightRequest = 0;
                YearRadio.TextColor = _unselectedTextColor;
                MonthRadio.TextColor = _selectedTextColor;
                AlltimeRadio.TextColor = _unselectedTextColor;
                break;
            case "This Year":
                range = FilterRange.Year;
                MonthUnderline.HeightRequest = 0;
                YearUnderline.HeightRequest = _underlineThickness;
                AllUnderline.HeightRequest = 0;
                YearRadio.TextColor = _selectedTextColor;
                MonthRadio.TextColor = _unselectedTextColor;
                AlltimeRadio.TextColor = _unselectedTextColor;
                break;
            case "All Time":
            default:
                range = FilterRange.AllTime;
                MonthUnderline.HeightRequest = 0;
                YearUnderline.HeightRequest = 0;
                AllUnderline.HeightRequest = _underlineThickness;
                YearRadio.TextColor = _unselectedTextColor;
                MonthRadio.TextColor = _unselectedTextColor;
                AlltimeRadio.TextColor = _selectedTextColor;
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