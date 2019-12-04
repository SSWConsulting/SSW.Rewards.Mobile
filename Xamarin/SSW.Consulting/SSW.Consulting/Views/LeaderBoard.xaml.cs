using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LeaderBoard : ContentPage
    {
        private DateTime typed;
        private DateTime show;
        private DateTime dismiss;
        private int lastItemIn;
        private bool showBar = true;

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
            show = DateTime.Now.AddMilliseconds(200);
            ((LeaderBoardViewModel)this.BindingContext).ScrollToMe = ((obj) =>
            {
                leaderList.ScrollTo(obj, ScrollToPosition.MakeVisible, true);
            });
        }

        private void Tapped(object sender, EventArgs e)
        {
            DisplayAlert("Tapped", "", "OK");
        }

        private void TextChanged(SearchBar sender, TextChangedEventArgs e)
        {
            typed = DateTime.Now;
            if (e.NewTextValue == null)
            {
                sender.Text = "";
            }
        }

        private void SearchButtonPress(object sender, EventArgs e)
        {
            UnfocusSearchBar(false);
        }

        private void UnfocusSearchBar(bool debounce)
        {
            dismiss = DateTime.Now;

            var shouldDismiss = debounce ? (DateTime.Now - dismiss).Duration().Milliseconds > 600 : true;
            if (searchView.IsFocused && shouldDismiss)
            {
                searchView.Unfocus();
            }
        }

        private void ToggleSearchBar()
        {
            var shouldDismiss = (DateTime.Now - show).Duration().Milliseconds > 200;
            if (!shouldDismiss)
            {
                return;
            }
            if (showBar)
            {
                 searchFrame.FadeTo(0, 100);
                 searchFrame.TranslateTo(0, -50, 150);
                 leaderList.Margin = new Thickness(0, 0, 0, 0);
                 showBar = false;
                 show = DateTime.Now;
            }
            else
            {
                 searchFrame.FadeTo(1, 150);
                 searchFrame.TranslateTo(0, 0, 100);
                 leaderList.Margin = new Thickness(0, 60, 0, 0);
                 showBar = true;
                 show = DateTime.Now;
            }
        }

        private void ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            
            if ((DateTime.Now - typed).Duration().Milliseconds > 200)
            {
                if (e.ItemIndex > lastItemIn)
                {
                    if(showBar){
                        UnfocusSearchBar(false);
                        ToggleSearchBar();
                    }
                }
                else
                {
                    if (!showBar)
                    {
                        UnfocusSearchBar(true);
                        ToggleSearchBar();
                    }
                }
                lastItemIn = e.ItemIndex;
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
