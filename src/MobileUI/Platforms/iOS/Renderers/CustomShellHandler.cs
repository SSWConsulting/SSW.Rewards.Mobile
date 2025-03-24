using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

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
}