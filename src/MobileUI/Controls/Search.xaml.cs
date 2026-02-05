
using System.Windows.Input;

namespace SSW.Rewards.Mobile.Controls;

public partial class Search
{
    private const string DismissIcon = "\ue4c3";
    private const string SearchIcon = "\uea7c";
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial Color BorderColor { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial Color TextColor { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial ICommand Command { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial bool ClearSearch { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty(PropertyChangedMethodName = nameof(OnPlaceholderChanged))]
    public partial string Placeholder { get; set; }
    
    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var placeholder = (string)newValue;
        var search = (Search)bindable;
        
        search.SearchEntry.Placeholder = placeholder;
    }
    
    [CommunityToolkit.Maui.BindableProperty(PropertyChangedMethodName = nameof(OnIsSearchingChanged))]
    public partial bool IsSearching { get; set; }

    private static void OnIsSearchingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var isSearching = (bool)newValue;
        var search = (Search)bindable;

        search.ActivityIndicator.IsVisible = isSearching;
        search.ActivityIndicator.IsRunning = isSearching;
        search.Icon.IsVisible = !isSearching;
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
}


