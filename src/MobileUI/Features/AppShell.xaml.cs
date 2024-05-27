using System.Diagnostics;

namespace SSW.Rewards.Mobile;

public partial class AppShell
{
    public AppShell(bool isStaff)
    {
        IsStaff = isStaff;

        BindingContext = this;
        InitializeComponent();
        Routing.RegisterRoute("earn/details", typeof(EarnDetailsPage));
        Routing.RegisterRoute("scan", typeof(ScanPage));
    }
    
    private bool _isStaff;
    public bool IsStaff
    {
        get => _isStaff;
        set
        {
            _isStaff = value;
            OnPropertyChanged();
        }
    }
    
    protected override bool OnBackButtonPressed()
    {
        if (Application.Current.MainPage.GetType() == typeof(AppShell) && Current.Navigation.NavigationStack.Where(x => x != null).Any())
        {
            return base.OnBackButtonPressed();
        }

        Process.GetCurrentProcess().CloseMainWindow();
        return true;
    }
}
