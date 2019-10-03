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