using System.Windows.Input;

namespace SSW.Rewards.Mobile.Controls;

public partial class TabHeader
{
    private Color _selectedTextColor;
    private Color _unselectedTextColor;
    private int _underlineThickness = 2;
    
    public static readonly BindableProperty CurrentPeriodProperty = BindableProperty.Create(
        nameof(CurrentPeriod),
        typeof(PeriodFilter),
        typeof(TabHeader),
        PeriodFilter.Week);

    public static readonly BindableProperty FilterChangedProperty = BindableProperty.Create(
        nameof(FilterChanged),
        typeof(ICommand),
        typeof(TabHeader));

    public TabHeader()
    {
        InitializeComponent();

        WeekUnderline.HeightRequest = _underlineThickness;
        MonthUnderline.HeightRequest = 0;
        YearUnderline.HeightRequest = 0;
        AllUnderline.HeightRequest = 0;

        var dict = App.Current.Resources.MergedDictionaries.First();

        _selectedTextColor = (Color)dict["primary"];
        _unselectedTextColor = (Color)dict["MutedText"];

        WeekRadio.TextColor = _selectedTextColor;
    }

    public PeriodFilter CurrentPeriod
    {
        get => (PeriodFilter)GetValue(CurrentPeriodProperty);
        set => SetValue(CurrentPeriodProperty, value);
    }

    public ICommand FilterChanged
    {
        get => (ICommand)GetValue(FilterChangedProperty);
        set => SetValue(FilterChangedProperty, value);
    }
    
    private void Item_Tapped(object sender, TappedEventArgs e)
    {
        var newPeriod = (PeriodFilter)e.Parameter;
        if (newPeriod == CurrentPeriod)
        {
            return;
        }

        CurrentPeriod = newPeriod;
        switch (CurrentPeriod)
        {
            case PeriodFilter.Month:
                WeekUnderline.HeightRequest = 0;
                MonthUnderline.HeightRequest = _underlineThickness;
                YearUnderline.HeightRequest = 0;
                AllUnderline.HeightRequest = 0;
                YearRadio.TextColor = _unselectedTextColor;
                MonthRadio.TextColor = _selectedTextColor;
                AlltimeRadio.TextColor = _unselectedTextColor;
                WeekRadio.TextColor = _unselectedTextColor;
                break;
            case PeriodFilter.Year:
                WeekUnderline.HeightRequest = 0;
                MonthUnderline.HeightRequest = 0;
                YearUnderline.HeightRequest = _underlineThickness;
                AllUnderline.HeightRequest = 0;
                YearRadio.TextColor = _selectedTextColor;
                MonthRadio.TextColor = _unselectedTextColor;
                AlltimeRadio.TextColor = _unselectedTextColor;
                WeekRadio.TextColor = _unselectedTextColor;
                break;
            case PeriodFilter.AllTime:
                WeekUnderline.HeightRequest = 0;
                MonthUnderline.HeightRequest = 0;
                YearUnderline.HeightRequest = 0;
                AllUnderline.HeightRequest = _underlineThickness;
                YearRadio.TextColor = _unselectedTextColor;
                MonthRadio.TextColor = _unselectedTextColor;
                AlltimeRadio.TextColor = _selectedTextColor;
                WeekRadio.TextColor = _unselectedTextColor;
                break;
            case PeriodFilter.Week:
            default:
                WeekUnderline.HeightRequest = _underlineThickness;
                MonthUnderline.HeightRequest = 0;
                YearUnderline.HeightRequest = 0;
                AllUnderline.HeightRequest = 0;
                YearRadio.TextColor = _unselectedTextColor;
                WeekRadio.TextColor = _selectedTextColor;
                MonthRadio.TextColor = _unselectedTextColor;
                AlltimeRadio.TextColor = _unselectedTextColor;
                break;
        }
        OnPropertyChanged(nameof(CurrentPeriod));
        FilterChanged?.Execute(null);
    }
}

public enum PeriodFilter
{
    Week,
    Month,
    Year,
    AllTime
}