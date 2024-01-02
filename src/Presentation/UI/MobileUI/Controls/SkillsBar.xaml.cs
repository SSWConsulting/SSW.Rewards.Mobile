namespace SSW.Rewards.Mobile.Controls
{
    public partial class SkillsBar : ContentView
    {
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(SkillsBar), null, propertyChanged: LabelChanged);

        public static readonly BindableProperty LevelProperty = BindableProperty.Create(nameof(Level), typeof(int), typeof(SkillsBar), 10, propertyChanged: LevelChanged);

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public int Level
        {
            get => (int)GetValue(LevelProperty);
            set => SetValue(LevelProperty, value);
        }

        public SkillsBar()
        {
            BindingContext = this;
            InitializeComponent();
        }

        private static void LevelChanged(BindableObject prop, object oldVal, object newVal)
        {
            var barControl = (SkillsBar)prop;

            switch (barControl.Level)
            {
                case 0:
                default:
                    barControl.Bar.TranslateTo(250, 0, 200, Easing.SinOut);
                    break;
                case 1:
                    barControl.Bar.TranslateTo(170, 0, 300, Easing.SinOut);
                    break;
                case 2:
                    barControl.Bar.TranslateTo(50, 0, 400, Easing.SinOut);
                    break;
            }
        }

        private static void LabelChanged(BindableObject prop, object oldVal, object newVal)
        {
            var barControl = (SkillsBar)prop;

            barControl.SkillLabel.Text = (string)newVal;
        }
    }
}