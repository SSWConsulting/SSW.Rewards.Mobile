using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace SSW.Rewards.Mobile.Renderers;

internal class CustomShellHandler : ShellRenderer
{
    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem item)
    {
        var renderer = new CustomShellItemRenderer(this) { ShellItem = item };

        // Override to not use new tab bar in iPadOS 18
        if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad &&
            UIDevice.CurrentDevice.CheckSystemVersion(18, 0) && renderer is ShellItemRenderer shellItemRenderer)
            shellItemRenderer.TraitOverrides.HorizontalSizeClass = UIUserInterfaceSizeClass.Compact;

        return renderer;
    }
}