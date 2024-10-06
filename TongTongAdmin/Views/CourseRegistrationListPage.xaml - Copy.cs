using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TongTongAdmin.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Reactive.Linq;
using Windows.UI.Core;
using System.ComponentModel;
using TongTongAdmin.Services;
using TongTongAdmin.Services.Interfaces;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TongTongAdmin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CourseRegistrationListPage : Page, INotifyPropertyChanged
    {
        private IFirebaseDatabaseService _database;

        public CourseRegistrationListPage()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CourseRegistration> CourseRegistrations { get; set; }

        public ObservableCollection<CourseRegistration> UnconfirmedCourseRegistrations
        {
            get
            {
                return CourseRegistrations == null ? null :
                    new ObservableCollection<CourseRegistration>(
                        CourseRegistrations
                            ?.Where(x => !x.IsConfirmed));
            }
        }

        public ObservableCollection<User> UsersThatHaveRegisteredForACourseBefore { get; set; }

        public ObservableCollection<Course> Courses { get; set; }

        public CourseRegistration SelectedRegistration { get; set; }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(CourseRegistrations == null)
            {
                _database = e.Parameter as IFirebaseDatabaseService;

                await LoadData();

                foreach(var registration in CourseRegistrations)
                {
                    registration.Registrant = UsersThatHaveRegisteredForACourseBefore
                        .FirstOrDefault(user => user.Uid == registration.Uid);
                }

                this.Bindings.Update();

                if(CourseRegistrationListView.SelectedIndex < 0 &&
                    UnconfirmedCourseRegistrations?.Count > 0)
                {
                    CourseRegistrationListView.SelectedIndex = 0;
                    SelectedRegistration = CourseRegistrationListView.SelectedItem as CourseRegistration;
                }
            }
        }

        public async Task LoadData()
        {
            var coursesTask = _database.GetCourses();
            var registrationsTask = _database.GetCourseRegistrations();
            var usersTask = _database.GetUsersThatHaveRegisteredForACourseBefore();

            try
            {
                await Task.WhenAll(coursesTask, registrationsTask, usersTask);

                Courses = coursesTask.Result;
                CourseRegistrations = registrationsTask.Result;
                UsersThatHaveRegisteredForACourseBefore = usersTask.Result;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }
        }

        private void CourseRegistrationListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedRegistration = e.ClickedItem as CourseRegistration;
        }

        private async void CourseChooserButton_Click(object sender, RoutedEventArgs e)
        {
            if(ExistingCourseRadioButton.IsChecked == true)
            {
                CourseChooserListView.ItemsSource = Courses.Where(x => x.ThisSemester == ThisSemesterRadioButton.IsChecked);
                ContentDialogResult result = await CourseChooserDialog.ShowAsync();

                if(result == ContentDialogResult.Primary)
                {
                    await AddUserToExistingCourse();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UnconfirmedCourseRegistrations"));
                }
            }
            else
            {
                ContentDialogResult result = await CreateCourseDialog.ShowAsync();

                if(result == ContentDialogResult.Primary)
                {
                    await AddUserToNewCourse(CourseTitleTextBox.Text, SelectedRegistration.CourseType, TuitionTextBox.Text);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UnconfirmedCourseRegistrations"));
                }
            }
        }

        private async Task AddUserToNewCourse(string courseTitle, string courseType, string tuition)
        {
            User registrant = SelectedRegistration.Registrant;

            bool thisSemester = ThisSemesterRadioButton.IsChecked == true;
            const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ssZ";
            var date = DateTimeOffset.Now;
            if(!thisSemester) date = date.AddMonths(1);
            string semester = date.ToString(DATE_FORMAT);
            Course newCourse = new Course
            {
                Title = courseTitle,
                CourseType = courseType,
                Tuition = tuition,
                ThisSemester = ThisSemesterRadioButton.IsChecked == true,
                Students = new Dictionary<string, string> { { registrant.Uid, registrant.Name } }
            };

            Courses.Add(newCourse);

            try
            {
                await _database.SaveCourse(newCourse);
                Task userTask = AddCourseDetailsToUser(newCourse.Id);
                Task registrationTask = UpdateRegistration();
                await Task.WhenAll(userTask, registrationTask);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }
        }

        private async Task AddUserToExistingCourse()
        {
            var selectedCourse = CourseChooserListView.SelectedItem as Course;
            selectedCourse.Students.Add(SelectedRegistration.Uid, SelectedRegistration.RegistrantName);
            selectedCourse.StudentUsers.Add(SelectedRegistration.Registrant);

            Task courseTask = _database.SaveCourse(selectedCourse);
            Task userTask = AddCourseDetailsToUser(selectedCourse.Id);
            Task registrationTask = UpdateRegistration();

            try
            {
                await Task.WhenAll(courseTask, userTask, registrationTask);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }
        }

        private Task UpdateRegistration()
        {
            SelectedRegistration.IsConfirmed = true;

            return _database.SaveCourseRegistration(SelectedRegistration);
        }

        private Task AddCourseDetailsToUser(string courseId)
        {
            User registrant = SelectedRegistration.Registrant;

            if(ThisSemesterRadioButton.IsChecked == true)
            {
                registrant.DidRegister = false;
            }

            registrant.CourseIdToPaidTuitionMap[courseId] = 0;

            return _database.SaveUser(registrant);
        }
    }
}