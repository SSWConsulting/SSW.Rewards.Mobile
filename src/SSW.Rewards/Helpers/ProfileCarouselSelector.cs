namespace SSW.Rewards.Mobile.Helpers;

public class ProfileCarouselSelector : DataTemplateSelector
{
    public DataTemplate Achievements { get; set; }
    public DataTemplate RecentActivity { get; set; }
    public DataTemplate Notifications { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var vm = (ProfileCarouselViewModel)item;

        switch (vm.Type)
        {
            case CarouselType.Achievements:
                return Achievements;
            case CarouselType.Notifications:
                return Notifications;
            case CarouselType.RecentActivity:
                return RecentActivity;
            default:
                throw new NotImplementedException("Carousel type not implemented");
        }
    }
}
