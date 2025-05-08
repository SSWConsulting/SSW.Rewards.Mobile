using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace SSW.Rewards.Mobile.Renderers;

internal class CustomShellHandler : ShellRenderer
{
    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
    {
        return new CustomShellItemRenderer(this);
    }

    protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
    {
        return new CustomShellSectionRenderer(this);
    }
}