using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TongTongAdmin.Services;
using TongTongAdmin.Services.Interfaces;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TongTongAdmin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IFirebaseDatabaseService _database;

        public MainPage()
        {
            this.InitializeComponent();
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ko";
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string firebaseAuthToken = await SignIn();
                _database = new FirebaseDatabaseService(firebaseAuthToken);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.ToString());
                var dialog = new MessageDialog(ex.ToString(), "Trouble Signing In");
                await dialog.ShowAsync();
            }

            CoursesPageListBoxItem.IsSelected = true;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CoursesPageListBoxItem.IsSelected)
            {
                MyFrame.Navigate(typeof(CourseListPage), _database);
                HeaderTextBlock.Text = "Courses";
            }
            else if(CourseRegistrationsPageListBoxItem.IsSelected)
            {
                MyFrame.Navigate(typeof(CourseRegistrationListPage), _database);
                HeaderTextBlock.Text = "Course Registrations";
            }
            else if(StudentsPageListBoxItem.IsSelected)
            {
                MyFrame.Navigate(typeof(StudentListPage), _database);
                HeaderTextBlock.Text = "Students";
            }

            MySplitView.IsPaneOpen = false;
        }

        private async Task<string> SignIn()
        {
            string oauthAccessToken = null;
            do
            {
                try
                {
                    oauthAccessToken = await GoogleAuthService.Client.SignInWithGoogleAsync();
                    if(string.IsNullOrEmpty(oauthAccessToken))
                    {
                        await DisplayTryAgainDialog();
                    }
                    else
                    {
                        break;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.ToString());
                    await DisplayTryAgainDialog();
                }
            } while(true);


            return await FirebaseAuthService.SignIn(oauthAccessToken);
        }

        private async Task DisplayTryAgainDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "Trouble Signing In",
                Content = "Must sign in before continuing.",
                PrimaryButtonText = "Try Again",
                CloseButtonText = "Exit App"
            };

            var result = await dialog.ShowAsync();

            if(result != ContentDialogResult.Primary)
            {
                Application.Current.Exit();
            }
        }
    }
}