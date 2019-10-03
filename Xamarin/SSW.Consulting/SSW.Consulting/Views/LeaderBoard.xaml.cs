using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSW.Consulting.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Consulting.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LeaderBoard : ContentPage
    {
        public LeaderBoard(LeaderBoardViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public LeaderBoard()
        {
            InitializeComponent();
            var vm = Resolver.Resolve<LeaderBoardViewModel>();
            vm.Navigation = Navigation;
            BindingContext = vm;
        }

        private void Tapped(object sender, EventArgs e)
        {
            DisplayAlert("Tapped", "","OK");
        }
    }
}