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
    public partial class LeaderSummary : ViewCell
    {
        public LeaderSummary()
        {

        }

        public LeaderSummary(LeaderSummaryViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}