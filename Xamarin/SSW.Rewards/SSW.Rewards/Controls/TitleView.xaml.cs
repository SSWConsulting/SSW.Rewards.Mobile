using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TitleView : ContentView
    {
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
                                                          propertyName: "TitleText",
                                                          returnType: typeof(string),
                                                          declaringType: typeof(TitleView),
                                                          defaultValue: "",
                                                          defaultBindingMode: BindingMode.TwoWay,
                                                          propertyChanged: TitlePropertyChanged);

        public string Title
        {
            get { return base.GetValue(TitleProperty).ToString(); }
            set { base.SetValue(TitleProperty, value); }
        }

        private static void TitlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (TitleView)bindable;
            control.title.Text = newValue.ToString();
        }

        public TitleView()
        {
            InitializeComponent();
        }
    }
}