namespace SSW.Rewards.Mobile.Renderers;

// Workaround for an issue with border animation on iOS
// See: https://github.com/dotnet/maui/issues/18204
class NoneAnimatedBorderContentView : Microsoft.Maui.Platform.ContentView
{
    public override void LayoutSubviews()
    {
        Layer.RemoveAllAnimations();

        if (Layer.Sublayers != null)
        {
            foreach (var subLayer in Layer.Sublayers)
            {
                subLayer.RemoveAllAnimations();
            }
        }

        base.LayoutSubviews();
    }
}