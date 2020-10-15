using Rg.Plugins.Popup.Pages;
using SSW.Rewards.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RewardDetailsPage : PopupPage
    {
        public RewardDetailsPage()
        {
            InitializeComponent();
        }

        public RewardDetailsPage(Reward reward)
        {

        }
    }
}