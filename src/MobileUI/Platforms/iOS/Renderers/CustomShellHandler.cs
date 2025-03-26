using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace SSW.Rewards.Mobile.Renderers;

internal class CustomShellHandler : ShellRenderer
{
    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem item)
    {
        return new CustomShellItemRenderer(this)
        {
            ShellItem = item
        };
    }
    
    private sealed class MyShellTabBarAppearanceTracker : ShellTabBarAppearanceTracker
    {
        public override void SetAppearance(UITabBarController controller, ShellAppearance appearance)
        {
            base.SetAppearance(controller, appearance);
            if (UIDevice.CurrentDevice.CheckSystemVersion(17, 0))
            {
                controller.TraitOverrides.HorizontalSizeClass = UIUserInterfaceSizeClass.Compact;

            }
        }
    }
    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker() => new MyShellTabBarAppearanceTracker();
}