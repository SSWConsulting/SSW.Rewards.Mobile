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
        private int lastItemOut;
        private int lastItemIn;

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


        private void SearchBarFocused(object sender,EventArgs e)
        {
            focus = DateTime.Now;

        }

        private async void SearchBarUnfocused(object sender, EventArgs e)
        {
            await searchBar.FadeTo(0.5, 450);
        }


        private void SearchUnfocus(object sender, EventArgs e)
        {
            UnfocusSearchBar();
        }

        private void UnfocusSearchBar()
        {
            var now = DateTime.Now;
            var shouldDismiss = (now - focus).Duration().Milliseconds > 300;
            if (searchBar.IsFocused && shouldDismiss)
            {
                searchBar.Unfocus();
            }
        }

        private void ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if(e.ItemIndex > lastItemIn )
            {
                Console.WriteLine("You are scrolling down");
                UnfocusSearchBar();

            }
                else
            {
                Console.WriteLine("You are scrolling Up");
                UnfocusSearchBar();
            }
            lastItemIn = e.ItemIndex;
        }
        private void ItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
            if (e.ItemIndex > lastItemOut)
            {
                //Console.WriteLine("You are scrolling up");
                //UnfocusSearchBar();

            }
            else
            {
                //Console.WriteLine("You are scrolling Down");
                //UnfocusSearchBar();
            }
            lastItemOut = e.ItemIndex;
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