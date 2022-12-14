using System.Windows.Input;

namespace SSW.Rewards.ViewModels
{
    public class LinkSocialViewModel : BaseViewModel
    {
        public string Platform { get; set; }

        public string Username { get; set; }

        public ICommand SaveUsernameCommand { get; set; }
    }
}
