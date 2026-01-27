namespace SSW.Rewards.Mobile.Pages;

public partial class PostDetailPage
{
    public PostDetailPage(PostDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
