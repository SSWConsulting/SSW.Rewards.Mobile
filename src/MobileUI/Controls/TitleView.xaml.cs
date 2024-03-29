﻿using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.Controls
{
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

        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            //App.Current.MainPage.DisplayAlert("Title bar tapped", "I'd tap that", "OK");
            WeakReferenceMessenger.Default.Send(new ScrollToTopMessage());
        }
    }
}