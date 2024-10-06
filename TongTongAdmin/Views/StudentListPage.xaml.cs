using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TongTongAdmin.Common;
using TongTongAdmin.Models;
using TongTongAdmin.Services.Interfaces;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TongTongAdmin.Views
{
    public sealed partial class StudentListPage : Page
    {
        private bool _didInit;
        private IFirebaseDatabaseService _database;

        public StudentListPage()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<User> UsersThatHaveRegisteredForACourseBefore { get; set; }

        public User SelectedStudent { get; set; }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(!_didInit)
            {
                _database = e.Parameter as IFirebaseDatabaseService;

                try
                {
                    UsersThatHaveRegisteredForACourseBefore = await _database.GetUsersThatHaveRegisteredForACourseBefore();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Trouble Loading Students");
                    await dialog.ShowAsync();
                }

                this.Bindings.Update();
                _didInit = true;

                if(UsersThatHaveRegisteredForACourseBefore?.Count > 0)
                {
                    StudentGridView.SelectedIndex = 0;
                    SelectedStudent = StudentGridView.SelectedItem as User;
                }
            }
        }

        private void StudentGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedStudent = e.ClickedItem as User;
        }

        private async void ViewCourseHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedStudent == null)
            {
                return;
            }

            var pastCourses = await _database.GetPastCourses(SelectedStudent.Uid);

            if(pastCourses == null || pastCourses.Count == 0)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Course History",
                    Content = "This user hasn't taken any courses.",
                    CloseButtonText = "OK"
                };

                await dialog.ShowAsync();
            }
            else
            {
                var allSyllabusItems = pastCourses
                    .Where(x => x.Syllabus != null)
                    .SelectMany(x => x.Syllabus.Values)
                    .OrderBy(x => x.FromTime);

                var groups = from c in allSyllabusItems
                             group c by c.Semester into g
                             select new { g.Key, Items = g };

                PastCourseCvs.Source = groups;
                ContentDialogResult result = await PastCoursesDialog.ShowAsync();
            }
        }

        private void EditCourseHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedStudent == null)
            {
                return;
            }

            var args = new DatabaseAndUidPageParams
            {
                Database = _database,
                Uid = SelectedStudent.Uid
            };

            this.Frame.Navigate(typeof(CourseHistoryEditPage), args);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedStudent == null)
            {
                return;
            }

            try
            {
                await _database.SaveUserPartial(SelectedStudent);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }
        }

        private async void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await AddStudentDialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                await AddStudent(StudentNameTextBox.Text, StudentPhoneNumTextBox.Text, StudentEmailTextBox.Text);
            }
        }

        private async void DeleteStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedStudent == null)
            {
                return;
            }

            if(SelectedStudent.Uid.Length > 20)
            {
                var dialog = new MessageDialog("Can't delete this student because this account was created by the user.", "Unable to delete");
                await dialog.ShowAsync();
                return;
            }

            var confirmationDialog = new ContentDialog
            {
                Title = "Delete student?",
                Content = "All student information will be lost.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "OK"
            };

            ContentDialogResult result = await confirmationDialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                try
                {
                    await _database.DeleteUser(SelectedStudent);
                    StudentGridView.UpdateLayout();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog("Oops. Failed to delete user.", "Error");
                    await dialog.ShowAsync();
                }
            }

            if(UsersThatHaveRegisteredForACourseBefore?.Count > 0)
            {
                StudentGridView.SelectedIndex = 0;
                SelectedStudent = StudentGridView.SelectedItem as User;
            }
        }

        private async Task AddStudent(string name, string phoneNum, string email)
        {
            User newStudent = new User
            {
                Name = name,
                PhoneNum = phoneNum,
                Email = email,
            };

            CourseRegistration registration = new CourseRegistration
            {
                RegistrantName = newStudent.Name,
                CourseType = "1-on-1 Tutoring",
                Location = "TongTong",
                IsConfirmed = true,
                TimestampMap = new Dictionary<string, long>
                {
                    { "timeStamp", DateTimeOffset.Now.ToUnixTimeSeconds() }
                }
            };

            try
            {
                Task newUserTask = _database.SaveNewUser(newStudent);
                Task photoUrlTask = newStudent.IsPhotoUrlNull();
                await Task.WhenAll(newUserTask, photoUrlTask);
                registration.Uid = newStudent.Uid;
                await _database.SaveCourseRegistration(registration);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }
        }
    }
}