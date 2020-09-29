using System.Windows.Input;
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

        public static readonly BindableProperty TapTitleCommandProperty = BindableProperty.Create("TapTitleCommand", typeof(ICommand), typeof(TitleView), null);

        public string Title
        {
            get { return base.GetValue(TitleProperty).ToString(); }
            set { base.SetValue(TitleProperty, value); }
        }

        public ICommand TapTitleCommand
        {
            get
            {
                return (ICommand)GetValue(TapTitleCommandProperty);
            }
            set
            {
                SetValue(TapTitleCommandProperty, value);
            }
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