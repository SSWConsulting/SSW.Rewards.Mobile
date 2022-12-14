using Rg.Plugins.Popup.Pages;
using SSW.Rewards.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

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