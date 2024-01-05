using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Mobile.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;
        
        [ObservableProperty]
        private string _title = string.Empty;

        public INavigation Navigation { get; set; }
    }
}
