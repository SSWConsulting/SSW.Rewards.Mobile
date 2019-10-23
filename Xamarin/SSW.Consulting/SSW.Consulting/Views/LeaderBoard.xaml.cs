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
        private DateTime lastRefreshed;
        private DateTime typed;
        private DateTime dismiss;
        private int lastItemIn;
        private int lastItemCount;
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

        private void ListRefreshed(object senbder,EventArgs e)
        {
            lastRefreshed = DateTime.Now;
        }

        private void UnfocusSearchBar(bool debounce)
        {
            var shouldDismiss = debounce ? (DateTime.Now - dismiss).Duration().Milliseconds > 600 : true;
            if (searchView.IsFocused && shouldDismiss)
            {
                searchView.Unfocus();
                dismiss = DateTime.Now;
            }
        }

        private void ToggleSearchBar()
        {
            if ((DateTime.Now - lastRefreshed).Duration().Milliseconds > 2000)
            {
                if (showBar)
                {
                    searchFrame.FadeTo(0, 100);
                    searchFrame.TranslateTo(0, -50, 150);
                    leaderList.Margin = new Thickness(0, 0, 0, 0);
                    showBar = false;
                }
                else
                {
                    searchFrame.FadeTo(1, 150);
                    searchFrame.TranslateTo(0, 0, 100);
                    leaderList.Margin = new Thickness(0, 60, 0, 0);
                    showBar = true;
                }
            }
        }

        private void ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            
            if ((DateTime.Now - typed).Duration().Milliseconds > 200)
            {
                if (e.ItemIndex > lastItemIn)
                {
                    if(showBar && !leaderList.IsRefreshing){
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
