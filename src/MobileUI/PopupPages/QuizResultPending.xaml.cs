using Mopups.Pages;

namespace SSW.Rewards.Mobile.PopupPages;

public partial class QuizResultPending : PopupPage
{
    private List<string> _loadingPhrases =
    [
        "Summoning answers from our quiz master!",
        "Extracting brilliance from our genius AI companion!",
        "Harvesting insights from the all-knowing AI guru!",
        "Retrieving wisdom from the depths of the AI genius!",
        "Snatching enlightenment from our brainy AI overlord!"
    ];
    
    public QuizResultPending()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadingText.Text = _loadingPhrases[new Random().Next(_loadingPhrases.Count)];
    }
}
