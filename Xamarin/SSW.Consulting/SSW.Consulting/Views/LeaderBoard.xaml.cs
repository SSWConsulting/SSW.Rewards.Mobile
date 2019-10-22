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
        private DateTime focus;
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
            focus = DateTime.Now;
            ((LeaderBoardViewModel)this.BindingContext).ScrollToMe = ((obj) =>
            {
                leaderList.ScrollTo(obj, ScrollToPosition.MakeVisible, true);
            });

        }

        private void Tapped(object sender, EventArgs e)
        {
            DisplayAlert("Tapped", "","OK");
        }


        private void SearchFocused(object sender,EventArgs e)
        {
            focus = DateTime.Now;
        }
        private void SearchUnfocus(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var shouldDismiss = (now - focus).Duration().Milliseconds > 600;
            if (searchBar.IsFocused && shouldDismiss)
            {
                searchBar.Unfocus();
            }
        }
            
        protected override void OnAppearing()
        {
            ((LeaderBoardViewModel)this.BindingContext).ScrollToMe = ((obj) =>
            {
                leaderList.ScrollTo(obj, ScrollToPosition.MakeVisible, true);
            });
            base.OnAppearing();
        }
    }
}