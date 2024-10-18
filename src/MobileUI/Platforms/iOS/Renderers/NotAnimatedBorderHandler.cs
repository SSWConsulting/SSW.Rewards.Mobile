using CoreAnimation;
using Microsoft.Maui.Handlers;
using ContentView = Microsoft.Maui.Platform.ContentView;

namespace SSW.Rewards.Mobile.Renderers;

// Workaround for an issue with border animation on iOS
// See: https://github.com/dotnet/maui/issues/18204
public class NotAnimatedBorderHandler : BorderHandler
{
    private class BorderContentView : ContentView
    {
        public override void LayoutSubviews()
        {
            Layer.RemoveAllAnimations();

            if (Layer.Sublayers != null)
            {
                foreach (var sublayer in Layer.Sublayers)
                {
                    sublayer.RemoveAllAnimations();
                }
            }

            base.LayoutSubviews();
        }
    }

    protected override ContentView CreatePlatformView()
    {
        _ = VirtualView ??
            throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ContentView)}");
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

        return new BorderContentView { CrossPlatformLayout = VirtualView };
    }
}