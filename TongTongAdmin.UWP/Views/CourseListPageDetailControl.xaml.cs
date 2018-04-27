using System;

using TongTongAdmin.UWP.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TongTongAdmin.UWP.Views
{
    public sealed partial class CourseListPageDetailControl : UserControl
    {
        public SampleOrder MasterMenuItem
        {
            get { return GetValue(MasterMenuItemProperty) as SampleOrder; }
            set { SetValue(MasterMenuItemProperty, value); }
        }

        public static readonly DependencyProperty MasterMenuItemProperty = DependencyProperty.Register("MasterMenuItem", typeof(SampleOrder), typeof(CourseListPageDetailControl), new PropertyMetadata(null, OnMasterMenuItemPropertyChanged));

        public CourseListPageDetailControl()
        {
            InitializeComponent();
        }

        private static void OnMasterMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CourseListPageDetailControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
