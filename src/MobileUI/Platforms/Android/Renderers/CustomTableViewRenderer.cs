using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using TableViewModelRenderer = Microsoft.Maui.Controls.Handlers.Compatibility.TableViewModelRenderer;
using TableViewRenderer = Microsoft.Maui.Controls.Handlers.Compatibility.TableViewRenderer;

namespace SSW.Rewards.Mobile.Renderers;

public class CustomTableViewRenderer : TableViewRenderer
{
    public CustomTableViewRenderer(Context context) : base(context)
    {
    }

    protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
    {
        base.OnElementChanged(e);
        if (Control == null)
            return;
    
        var listView = Control;
        listView.Divider = new ColorDrawable(Colors.Transparent.ToPlatform());
    }

    protected override TableViewModelRenderer GetModelRenderer(Android.Widget.ListView listView, TableView view)
    {
        return new CustomHeaderTableViewModelRenderer(Context, listView, view);
    }

    private class CustomHeaderTableViewModelRenderer : TableViewModelRenderer
    {
        public CustomHeaderTableViewModelRenderer(Context context, Android.Widget.ListView listView, TableView view) :
            base(context, listView, view)
        {
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, ViewGroup parent)
        {
            var view = base.GetView(position, convertView, parent);

            var element = GetCellForPosition(position);

            // section header will be a TextCell
            if (element.GetType() == typeof(TextCell))
            {
                try
                {
                    // get the divider below the header
                    var divider = (view as LinearLayout)?.GetChildAt(1);

                    // Set the color
                    divider?.SetBackgroundColor(Colors.Transparent.ToPlatform());
                }
                catch (Exception) { }
            }

            return view;
        }
    }
}