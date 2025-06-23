﻿
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
    
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(Search),
        propertyChanged: OnPlaceholderChanged);

    private static void OnPlaceholderChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var placeholder = (string)newvalue;
        var search = (Search)bindable;
        
        search.SearchEntry.Placeholder = placeholder;
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    
    public static readonly BindableProperty IsSearchingProperty = BindableProperty.Create(
        nameof(IsSearching),
        typeof(bool),
        typeof(Search),
        propertyChanged: OnIsSearchingChanged);

    private static void OnIsSearchingChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var isSearching = (bool)newvalue;
        var search = (Search)bindable;

        search.ActivityIndicator.IsRunning = isSearching;
        search.Icon.IsVisible = !isSearching;
    }

    public bool IsSearching
    {
        get => (bool)GetValue(IsSearchingProperty);
        set => SetValue(IsSearchingProperty, value);
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


