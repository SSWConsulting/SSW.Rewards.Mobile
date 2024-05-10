using System.Windows.Input;

namespace SSW.Rewards.Mobile.Controls;

public partial class MultiLineButton
{
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(MultiLineButton),
            string.Empty
        );
    
    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(MultiLineButton),
            Colors.White
        );
    
    public int FontSize
    {
        get => (int)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(int),
            typeof(MultiLineButton),
            14
        );
    
    public new Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public new static readonly BindableProperty BackgroundColorProperty =
        BindableProperty.Create(
            nameof(BackgroundColor),
            typeof(Color),
            typeof(MultiLineButton),
            App.Current.Resources["SSWRed"] as Color
        );
    
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(MultiLineButton)
        );
    
    public MultiLineButton()
    {
        InitializeComponent();
    }
}