
using System.Windows.Input;

namespace SSW.Rewards.Mobile.Controls;

public partial class Search
{
    private const string DismissIcon = "\ue4c3";
    private const string SearchIcon = "\uea7c";

    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor),
        typeof(Color),
        typeof(Search));
    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(Search));

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(Search));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty ClearSearchProperty = BindableProperty.Create(
        nameof(ClearSearch),
        typeof(bool),
        typeof(Search));

    public bool ClearSearch
    {
        get => (bool)GetValue(ClearSearchProperty);
        set => SetValue(ClearSearchProperty, value);
    }

    public Search()
    {
        InitializeComponent();
        Icon.Text = SearchIcon;
    }

    private void SearchEntry_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue;
        Icon.Text = string.IsNullOrEmpty(searchText) ? SearchIcon : DismissIcon;
    }

    private void Icon_OnTapped(object sender, EventArgs e)
    {
        Clear();
    }

    private void Clear()
    {
        SearchEntry.Text = string.Empty;
        Icon.Text = SearchIcon;
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(BorderColor))
        {
            SearchControl.BackgroundColor = BorderColor;
        }
        else if (propertyName == nameof(TextColor))
        {
            SearchEntry.TextColor = TextColor;
            Icon.TextColor = TextColor;
        }
        else if (propertyName == nameof(ClearSearch))
        {
            Clear();
        }
    }
}


