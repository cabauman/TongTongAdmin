using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TongTongAdmin.Helpers;
using TongTongAdmin.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Diagnostics;
using TongTongAdmin.Services.Interfaces;
using Windows.UI.Popups;
using TongTongAdmin.Common;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using TongTongAdmin.Services;
using Windows.Storage.Provider;
//using ClosedXML.Excel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TongTongAdmin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CourseListPage : Page, INotifyPropertyChanged
    {
        private IFirebaseDatabaseService _database;
        private Course _selectedCourse;

        public event PropertyChangedEventHandler PropertyChanged;

        public Course SelectedCourse
        {
            get { return _selectedCourse; }
            set { _selectedCourse = value; PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SelectedCourse")); }
        }

        public ObservableCollection<Course> Courses { get; set; }

        public IReadOnlyList<Teacher> TeacherPool { get; set; }

        public ObservableCollection<CourseRegistration> CourseRegistrations { get; set; }

        public ObservableCollection<User> UsersThatHaveRegisteredForACourseBefore { get; set; }

        public string SelectedPivotItem { get; set; }

        public SyllabusItem SelectedSyllabusItem
        {
            get { return SelectedCourse?.SelectedSyllabusItem; }
        }

        public ScheduleItem SelectedScheduleItem
        {
            get { return SelectedCourse?.SelectedScheduleItem; }
        }

        public User SelectedStudent
        {
            get { return SelectedCourse?.SelectedStudent; }
        }

        public CourseListPage()
        {
            this.InitializeComponent();
        }

        private async Task ReadExcelFileTest()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".xlsx");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if(file != null)
            {
                // Retrieve the value in cell A1.
                string value = await Sample1.GetCellValue(file.Path, "Sheet1", "A1", file);
                Console.WriteLine(value);
                // Retrieve the date value in cell A2.
                value = await Sample1.GetCellValue(file.Path, "Sheet1", "B2", file);
                Console.WriteLine(value);
            }
        }

        private async Task CreateExcelFileTest()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/ExcelTemplate.xlsx"));
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xlsx" });
            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            savePicker.SuggestedFileName = "TongTongArchive";
            var saveFile = await savePicker.PickSaveFileAsync();
            await file.CopyAndReplaceAsync(saveFile);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //await CreateExcelFileTest();
            //await ReadExcelFileTest();

            //return;

            if(Courses == null)
            {
                _database = e.Parameter as IFirebaseDatabaseService;

                var coursesTask = _database.GetCourses();
                var usersTask = _database.GetUsersThatHaveRegisteredForACourseBefore();
                var teachersTask = _database.GetTeachers();

                var loadingDialog = new ContentDialog
                {
                    Title = "Loading...",
                    Content = new ProgressRing() { IsActive = true, Width = 80, Height = 80 }
                };

                var loadingDialogTask = loadingDialog.ShowAsync();

                try
                {
                    await Task.WhenAll(coursesTask, usersTask, teachersTask);
                    // Course registrations are loaded by GetUsersThatHaveRegisteredForACourseBefore.
                    // So the following call will just retrieve that result. 
                    CourseRegistrations = await _database.GetCourseRegistrations();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Error");
                    await dialog.ShowAsync();
                }

                Courses = coursesTask.Result;
                UsersThatHaveRegisteredForACourseBefore = usersTask.Result;
                TeacherPool = teachersTask.Result;

                SyncGroupedCourseListView();

                if(Courses.Count > 0)
                {
                    this.Bindings.Update();
                    await HandleSelectedCourse(Courses[0]);
                }

                List<Task> tasks = new List<Task>();
                foreach(var user in UsersThatHaveRegisteredForACourseBefore)
                {
                    tasks.Add(user.IsPhotoUrlNull());
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Error");
                    await dialog.ShowAsync();
                }
                finally
                {
                    loadingDialog.Hide();
                }

                await loadingDialogTask;
            }
            else
            {
                SyncGroupedCourseListView();
            }
        }

        private async void CourseListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            await HandleSelectedCourse(e.ClickedItem as Course);
        }

        private async Task HandleSelectedCourse(Course course)
        {
            SelectedCourse = course;

            if(!course.DidInit)
            {
                await InitCourse(course);
                this.Bindings.Update();
            }

            if(SyllabusItemListView.SelectedIndex < 0 &&
                SelectedCourse.SyllabusItems?.Count > 0)
            {
                SyllabusItemListView.SelectedIndex = 0;
                SelectedCourse.SelectedSyllabusItem = SyllabusItemListView.SelectedItem as SyllabusItem;
            }

            if(ScheduleItemListView.SelectedIndex < 0 &&
                SelectedCourse.ScheduleItems?.Count > 0)
            {
                ScheduleItemListView.SelectedIndex = 0;
                SelectedCourse.SelectedScheduleItem = ScheduleItemListView.SelectedItem as ScheduleItem;
            }

            if(StudentGridView.SelectedIndex < 0 &&
                SelectedCourse.StudentUsers?.Count > 0)
            {
                StudentGridView.SelectedIndex = 0;
                SelectedCourse.SelectedStudent = StudentGridView.SelectedItem as User;
            }

            CoursePivot_SelectionChanged(CoursePivot, null);
        }

        private async Task InitCourse(Course course)
        {
            Task<ObservableCollection<SyllabusItem>> syllabusItemsTask = _database.GetSyllabusItems(course.Id);
            Task<ObservableCollection<ScheduleItem>> scheduleItemsTask = _database.GetScheduleItems(course.Id);

            try
            {
                await Task.WhenAll(syllabusItemsTask, scheduleItemsTask);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }

            course.SyllabusItems = syllabusItemsTask.Result;
            course.ScheduleItems = scheduleItemsTask.Result;

            var students = UsersThatHaveRegisteredForACourseBefore
                .Where(user => user.CourseId == course.Id || user.NextSemesterCourseId == course.Id);
            course.StudentUsers = new ObservableCollection<User>(students);

            foreach(var student in course.StudentUsers)
            {
                student.Course = course;
            }

            foreach(var item in course.ScheduleItems)
            {
                item.PropertyChanged += ScheduleInfoChanged;
            }

            course.DidInit = true;
        }

        private void SyllabusItemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedCourse.SelectedSyllabusItem = e.ClickedItem as SyllabusItem;
        }

        private void ScheduleItemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedCourse.SelectedScheduleItem = e.ClickedItem as ScheduleItem;
        }

        private void StudentGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedCourse.SelectedStudent = e.ClickedItem as User;
        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch(SelectedPivotItem)
            {
                case "Syllabus":
                    AddSyllabusItem();
                    break;
                case "Schedule":
                    AddScheduleItem();
                    break;
                case "Students":
                    AddStudent();
                    break;
            }

            SaveAppBarButton.IsEnabled = true;
        }

        private async void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch(SelectedPivotItem)
            {
                case "Syllabus":
                    await SaveSyllabusItem();
                    break;
                case "Schedule":
                    await SaveScheduleItem();
                    break;
                case "Students":
                    await SaveStudent();
                    break;
                case "Details":
                    await SaveCourseDetails();
                    break;
            }
        }

        private async void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch(SelectedPivotItem)
            {
                case "Syllabus":
                    await DeleteSyllabusItem();
                    break;
                case "Schedule":
                    await DeleteScheduleItem();
                    break;
                case "Students":
                    await RemoveStudentFromCourse();
                    break;
            }
        }

        private void AddSyllabusItem()
        {
            SelectedCourse.SyllabusItems.Add(new SyllabusItem());
            SyllabusItemListView.SelectedIndex = SelectedCourse.SyllabusItems.Count - 1;
            SelectedCourse.SelectedSyllabusItem = SyllabusItemListView.SelectedItem as SyllabusItem;
        }

        private async Task SaveSyllabusItem()
        {
            try
            {
                await _database.SaveSyllabusItem(SelectedSyllabusItem, SelectedCourse.Id);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }
        }

        private async Task DeleteSyllabusItem()
        {
            if(SelectedSyllabusItem == null)
            {
                return;
            }

            var confirmationDialog = new ContentDialog
            {
                Title = "Delete syllabus item?",
                Content = "This syllabus item will be deleted.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "OK"
            };

            ContentDialogResult result = await confirmationDialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                try
                {
                    await _database.DeleteSyllabusItem(SelectedSyllabusItem, SelectedCourse.Id);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Error");
                    await dialog.ShowAsync();
                }

                SelectedCourse.SyllabusItems.RemoveAt(SyllabusItemListView.SelectedIndex);
                if(SelectedCourse.SyllabusItems.Count > 0)
                {
                    SyllabusItemListView.SelectedIndex = 0;
                    SelectedCourse.SelectedSyllabusItem = SyllabusItemListView.SelectedItem as SyllabusItem;
                }
            }
        }

        private void AddScheduleItem()
        {
            var newScheduleItem = new ScheduleItem(TeacherPool);
            SelectedCourse.ScheduleItems.Add(newScheduleItem);
            ScheduleItemListView.SelectedIndex = SelectedCourse.ScheduleItems.Count - 1;
            SelectedCourse.SelectedScheduleItem = newScheduleItem;
            newScheduleItem.PropertyChanged += ScheduleInfoChanged;
        }

        private async Task SaveScheduleItem()
        {
            try
            {
                // We have to save the course, too, in order to update the DayToTimeMap
                Task courseTask = _database.SaveCourse(SelectedCourse);
                Task scheduleItemTask = _database.SaveScheduleItem(SelectedScheduleItem, SelectedCourse.Id);
                await Task.WhenAll(courseTask, scheduleItemTask);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }
        }

        private async Task DeleteScheduleItem()
        {
            if(SelectedScheduleItem == null)
            {
                return;
            }

            var confirmationDialog = new ContentDialog
            {
                Title = "Delete schedule item?",
                Content = "This schedule item will be deleted.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "OK"
            };

            ContentDialogResult result = await confirmationDialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                try
                {
                    await _database.DeleteScheduleItem(SelectedScheduleItem, SelectedCourse.Id);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Error");
                    await dialog.ShowAsync();
                }

                SelectedCourse.ScheduleItems.RemoveAt(ScheduleItemListView.SelectedIndex);
                if(SelectedCourse.ScheduleItems.Count > 0)
                {
                    ScheduleItemListView.SelectedIndex = 0;
                    SelectedCourse.SelectedScheduleItem = ScheduleItemListView.SelectedItem as ScheduleItem;
                }
            }
        }

        private async void AddStudent()
        {
            if(SelectedCourse.ThisSemester)
            {
                StudentChooserListView.ItemsSource = UsersThatHaveRegisteredForACourseBefore
                    .Where(user => string.IsNullOrEmpty(user.CourseId));
            }
            else
            {
                StudentChooserListView.ItemsSource = UsersThatHaveRegisteredForACourseBefore
                    .Where(user => string.IsNullOrEmpty(user.NextSemesterCourseId));
            }

            ContentDialogResult result = await StudentChooserDialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                try
                {
                    await AddUserToSelectedCourse();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Error");
                    await dialog.ShowAsync();
                }

                StudentGridView.SelectedIndex = SelectedCourse.Students.Count - 1;
                SelectedCourse.SelectedStudent = StudentGridView.SelectedItem as User;
            }
        }

        private async Task AddUserToSelectedCourse()
        {
            var selectedUser = StudentChooserListView.SelectedItem as User;
            SelectedCourse.Students.Add(selectedUser.Uid, selectedUser.Name);
            SelectedCourse.StudentUsers.Add(selectedUser);

            Task courseTask = _database.SaveCourse(SelectedCourse);
            Task userTask = AddCourseDetailsToUser(selectedUser, SelectedCourse);
            Task registrationTask = UpdateRegistration(selectedUser);

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

        private Task AddCourseDetailsToUser(User user, Course course)
        {
            user.HasCurrentOrUpcomingCourse = true;

            if(course.ThisSemester)
            {
                user.DidRegister = false;
                user.CourseType = course.Title;
                user.CourseId = course.Id;
            }
            else
            {
                user.DidRegister = false;
                user.NextSemesterCourseType = course.Title;
                user.NextSemesterCourseId = course.Id;
            }

            return _database.SaveUser(user);
        }

        private Task UpdateRegistration(User user)
        {
            var registration = CourseRegistrations.First(reg => reg.Uid == user.Uid);
            registration.IsConfirmed = true;

            return _database.SaveCourseRegistration(registration);
        }

        private async Task SaveStudent()
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

        private async Task RemoveStudentFromCourse()
        {
            if(SelectedStudent == null)
            {
                return;
            }

            var confirmationDialog = new ContentDialog
            {
                Title = "Remove student?",
                Content = "This student will be removed from the course.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "OK"
            };

            ContentDialogResult result = await confirmationDialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                SelectedStudent.RemoveCourse(SelectedCourse.ThisSemester);

                try
                {
                    await _database.RemoveStudentFromCourse(SelectedStudent, SelectedCourse.Id);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Error");
                    await dialog.ShowAsync();
                }

                SelectedCourse.RemoveSelectedStudent();
                if(SelectedCourse.StudentUsers.Count > 0)
                {
                    StudentGridView.SelectedIndex = 0;
                    SelectedCourse.SelectedStudent = StudentGridView.SelectedItem as User;
                }
            }
        }

        private async Task SaveCourseDetails()
        {
            try
            {
                await _database.SaveCourse(SelectedCourse);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }
        }

        private void CoursePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivotItem = CoursePivot.SelectedItem as PivotItem;
            SelectedPivotItem = pivotItem.Header.ToString();

            switch(SelectedPivotItem)
            {
                case "Syllabus":
                    AddAppBarButton.Visibility = Visibility.Visible;
                    DeleteAppBarButton.Visibility = Visibility.Visible;
                    DeleteAppBarButton.IsEnabled = SyllabusItemListView.SelectedIndex != -1;
                    SaveAppBarButton.IsEnabled = SyllabusItemListView.SelectedIndex != -1;
                    break;
                case "Schedule":
                    AddAppBarButton.Visibility = Visibility.Visible;
                    DeleteAppBarButton.Visibility = Visibility.Visible;
                    DeleteAppBarButton.IsEnabled = ScheduleItemListView.SelectedIndex != -1;
                    SaveAppBarButton.IsEnabled = ScheduleItemListView.SelectedIndex != -1;
                    break;
                case "Students":
                    AddAppBarButton.Visibility = Visibility.Visible;
                    DeleteAppBarButton.Visibility = Visibility.Visible;
                    DeleteAppBarButton.IsEnabled = StudentGridView.SelectedIndex != -1;
                    SaveAppBarButton.IsEnabled = StudentGridView.SelectedIndex != -1;
                    break;
                case "Details":
                    AddAppBarButton.Visibility = Visibility.Collapsed;
                    DeleteAppBarButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async void CreateCourseButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await CreateCourseDialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                Course newCourse = new Course
                {
                    Title = CourseTitleTextBox.Text,
                    CourseType = "1-on-1 Tutoring",
                    Tuition = TuitionTextBox.Text,
                    ThisSemester = true,
                    Students = new Dictionary<string, string>()
                };
                
                try
                {
                    await _database.SaveCourse(newCourse);
                    Courses.Add(newCourse);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog("Oops. Failed to create new course.", "Error");
                    await dialog.ShowAsync();
                }

                SyncGroupedCourseListView();
            }
        }

        private async void DeleteCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmationDialog = new ContentDialog
            {
                Title = "Delete course?",
                Content = "This course will be deleted.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "OK"
            };

            ContentDialogResult result = await confirmationDialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                try
                {
                    await _database.DeleteCourse(SelectedCourse);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Error");
                    await dialog.ShowAsync();
                }

                Courses.RemoveAt(CourseListView.SelectedIndex);
                if(Courses.Count > 0)
                {
                    CourseListView.SelectedIndex = 0;
                    SelectedCourse = CourseListView.SelectedItem as Course;
                }

                SyncGroupedCourseListView();
            }
        }

        private void SyncGroupedCourseListView()
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string thisSemesterText = resourceLoader.GetString("ThisSemester/Content");
            string nextSemesterText = resourceLoader.GetString("NextSemester/Content");
            var groups = from c in Courses
                         group c by c.ThisSemester into g
                         select new { Key = g.Key == true ? thisSemesterText : nextSemesterText, Items = g };

            Cvs.Source = groups;

            if(Courses.Count > 0)
            {
                CourseListView.SelectedIndex = 0;
                SelectedCourse = CourseListView.SelectedItem as Course;
            }
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

        private void ScheduleInfoChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "FromTime" || e.PropertyName == "ToTime")
            {
                if(SelectedCourse.DayToTimeMap == null)
                {
                    SelectedCourse.DayToTimeMap = new Dictionary<string, object>();
                }

                if(SelectedScheduleItem.DayKeys == null)
                {
                    SelectedScheduleItem.DayKeys = new List<string>();
                }

                foreach(var dayKey in SelectedScheduleItem.DayKeys)
                {
                    SelectedCourse.DayToTimeMap[dayKey] = new Dictionary<string, string>
                    {
                        { "fromTime", SelectedScheduleItem.FromTime },
                        { "toTime", SelectedScheduleItem.ToTime },
                    };
                }
            }
            else if(e.PropertyName == "Days")
            {
                if(SelectedCourse.DayToTimeMap == null)
                {
                    SelectedCourse.DayToTimeMap = new Dictionary<string, object>();
                }

                string dayKey = ScheduleItem.DayFlagToDayKeyMap[SelectedScheduleItem.LastModifiedDay];
                if(SelectedScheduleItem.SelectedDays.HasFlag(SelectedScheduleItem.LastModifiedDay))
                {
                    SelectedCourse.DayToTimeMap[dayKey] = new Dictionary<string, string>
                    {
                        { "fromTime", SelectedScheduleItem.FromTime },
                        { "toTime", SelectedScheduleItem.ToTime },
                    };
                }
                else
                {
                    // Only remove from the DayToTimeMap if it's NOT contained in another schedule item.
                    var result = SelectedCourse.ScheduleItems
                        .Where(x => x != SelectedScheduleItem)
                        .Select(x => x.DayKeys)
                        .FirstOrDefault(x => x.Contains(dayKey));

                    if(result == null)
                    {
                        SelectedCourse.DayToTimeMap[dayKey] = null;
                    }
                }
            }
        }

        private async void StartNewSemesterButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmationDialog = new ContentDialog
            {
                Title = "Start new semester?",
                Content = "All courses under 'this semester' will be deleted, and next semester's courses will be moved to this semester.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "OK"
            };

            ContentDialogResult result = await confirmationDialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                Task task = _database.StartNewSemester();
                //Task task = _database.AddToCourseHistory(SelectedCourse);
                try
                {
                    await task;
                }
                catch(Exception ex)
                {
                    List<string> exceptions = new List<string> { ex.Message };
                    exceptions.Add("Task IsFaulted: " + task.IsFaulted);
                    foreach(var inEx in task.Exception.InnerExceptions)
                    {
                        exceptions.Add("Task Inner Exception: " + inEx.Message);
                    }

                    var dialog = new MessageDialog(string.Join("\n", exceptions), "Error");
                    await dialog.ShowAsync();
                }

                SyncGroupedCourseListView();

                if(Courses.Count > 0)
                {
                    CourseListView.SelectedIndex = 0;
                    SelectedCourse = CourseListView.SelectedItem as Course;
                }
            }
        }

        private async void RenewAllCoursesButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmationDialog = new ContentDialog
            {
                Title = "Renew all courses?",
                Content = "All courses under 'this semester' will be duplicated and placed under 'next semester.'",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "OK"
            };

            ContentDialogResult result = await confirmationDialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                Task task = _database.RenewAllCourses();
                try
                {
                    await task;
                }
                catch(Exception ex)
                {
                    List<string> exceptions = new List<string> { ex.Message };
                    exceptions.Add("Task IsFaulted: " + task.IsFaulted);
                    foreach(var inEx in task.Exception.InnerExceptions)
                    {
                        exceptions.Add("Task Inner Exception: " + inEx.Message);
                    }

                    var dialog = new MessageDialog(string.Join("\n", exceptions), "Error");
                    await dialog.ShowAsync();
                }

                SyncGroupedCourseListView();

                if(Courses.Count > 0)
                {
                    CourseListView.SelectedIndex = 0;
                    SelectedCourse = CourseListView.SelectedItem as Course;
                }
            }
        }

        private async void RenewCourseButton_Click(object sender, RoutedEventArgs e)
        {
            await _database.RenewCourse(SelectedCourse);
            SyncGroupedCourseListView();
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

        private async void ExportExcelFileButton_Click(object sender, RoutedEventArgs e)
        {
            var templateFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/ExcelTemplate.xlsx"));
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xlsx" });
            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            savePicker.SuggestedFileName = "TongTongArchive";
            var saveFile = await savePicker.PickSaveFileAsync();

            if(saveFile != null)
            {
                var loadingDialog = new ContentDialog
                {
                    Title = "Exporting...",
                    Content = new ProgressRing() { IsActive = true, Width = 80, Height = 80 }
                };

                var loadingDialogTask = loadingDialog.ShowAsync();

                CachedFileManager.DeferUpdates(saveFile);

                try
                {
                    await templateFile.CopyAndReplaceAsync(saveFile);

                    var courseTasks = new List<Task>();
                    foreach(var course in Courses)
                    {
                        if(!course.DidInit)
                        {
                            courseTasks.Add(InitCourse(course));
                        }
                    }

                    await Task.WhenAll(courseTasks);

                    List<ICourse> allCourses = new List<ICourse>(Courses);
                    var tasks = new List<Task<ObservableCollection<PastCourse>>>();
                    foreach(var user in UsersThatHaveRegisteredForACourseBefore)
                    {
                        tasks.Add(_database.GetPastCourses(user.Uid));
                    }

                    var result = await Task.WhenAll(tasks);
                    var allPastCourses = result.SelectMany(x => x);
                    allCourses.AddRange(allPastCourses);

                    await CourseArchiveExporterService.ExportExcelFile(allCourses, "Sheet1", saveFile);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.ToString());
                    var dialog = new MessageDialog(ex.ToString(), "Error");
                    await dialog.ShowAsync();
                }
                finally
                {
                    loadingDialog.Hide();
                }

                await loadingDialogTask;

                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(saveFile);
                if(status == FileUpdateStatus.Complete)
                {
                    NotificationHelper.ShowToast("Export Complete", "File " + saveFile.Name + " was saved.");
                }
                else
                {
                    NotificationHelper.ShowToast("Export Failed", "File " + saveFile.Name + " couldn't be saved.");
                }
            }
        }

        private void DigitTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if(!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
            {
                e.Handled = true;
            }
        }
    }
}