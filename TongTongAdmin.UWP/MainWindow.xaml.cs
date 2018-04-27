using ReactiveUI;
using Splat;
using TongTongAdmin.ViewModels;
using Windows.UI.Xaml.Controls;

namespace TongTongAdmin.UWP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Page
    {
        public AppBootstrapper AppBootstrapper { get; protected set; }

        public MainWindow()
        {
            InitializeComponent();

            //DataContext = Locator.Current.GetService(typeof(IScreen));
            AppBootstrapper = new AppBootstrapper();
            DataContext = AppBootstrapper;
        }
    }
}
