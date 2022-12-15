using Mopups.Pages;

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