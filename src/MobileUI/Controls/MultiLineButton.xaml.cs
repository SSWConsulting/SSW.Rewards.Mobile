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