using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using TongTongAdmin.Common;
using TongTongAdmin.Models;
using TongTongAdmin.Services.Interfaces;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TongTongAdmin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CourseHistoryEditPage : Page, INotifyPropertyChanged
    {
        private IFirebaseDatabaseService _database;
        private PastCourse _selectedCourse;
        private string _uid;

        public CourseHistoryEditPage()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<PastCourse> PastCourses { get; private set; }

        public PastCourse SelectedCourse
        {
            get { return _selectedCourse; }
            set
            {
                _selectedCourse = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SelectedCourse"));
            }
        }

        public SyllabusItem SelectedSyllabusItem { get; private set; }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(_database == null)
            {
                var args = e.Parameter as DatabaseAndUidPageParams;
                _database = args.Database;
                _uid = args.Uid;

                try
                {
                    PastCourses = await _database.GetPastCourses(_uid);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Trouble Loading Past Courses");
                    await dialog.ShowAsync();
                }

                Bindings.Update();

                if(PastCourses?.Count > 0)
                {
                    PastCourseListView.SelectedIndex = 0;
                    SelectedCourse = PastCourseListView.SelectedItem as PastCourse;
                }

                Bindings.Update();

                if(SelectedCourse?.Syllabus?.Count > 0)
                {
                    SyllabusItemListView.SelectedIndex = 0;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void PastCourseListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedCourse = e.ClickedItem as PastCourse;
            SelectedCourse.Uid = _uid;
            if(SelectedCourse.Syllabus?.Count > 0)
            {
                SyllabusItemListView.SelectedIndex = 0;
                SelectedSyllabusItem = SyllabusItemListView.SelectedItem as SyllabusItem;
            }
        }

        private void SyllabusItemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedSyllabusItem = e.ClickedItem as SyllabusItem;
        }

        private async void AddSyllabusItemButton_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedCourse == null)
            {
                return;
            }

            var syllabusItem = new SyllabusItem();
            // We only need FromTime because this just marks the date.
            // Past course doesn't keep track of how long a class is.
            syllabusItem.ToTime = null;

            try
            {
                await _database.AddPastCourseSyllabusItem(syllabusItem, _uid, SelectedCourse.Id);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
            }

            SelectedCourse.Syllabus.Add(syllabusItem.Id, syllabusItem);
            SelectedCourse.SyllabusItems.Add(syllabusItem);
            SyllabusItemListView.SelectedIndex = SelectedCourse.Syllabus.Count - 1;
            SelectedSyllabusItem = syllabusItem;
        }

        private async void DeleteSyllabusItemButton_Click(object sender, RoutedEventArgs e)
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
                SelectedCourse.Syllabus.Remove(SelectedSyllabusItem.Id);
                SelectedCourse.SyllabusItems.RemoveAt(SyllabusItemListView.SelectedIndex);

                try
                {
                    await _database.SavePastCourse(SelectedCourse, _uid);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Exception Message: " + ex.Message);
                    var dialog = new MessageDialog(ex.Message, "Error");
                    await dialog.ShowAsync();
                }

                if(SelectedCourse.Syllabus.Count > 0)
                {
                    SyllabusItemListView.SelectedIndex = 0;
                    SelectedSyllabusItem = SyllabusItemListView.SelectedItem as SyllabusItem;
                }
            }
        }

        private async void SaveCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedCourse == null)
            {
                return;
            }

            // If the user doesn't click out of the edited textbox, that text
            // doesn't get saved, so this handles that case.
            SaveAppBarButton.Focus(FocusState.Programmatic);

            try
            {
                await _database.SavePastCourse(SelectedCourse, _uid);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception Message: " + ex.Message);
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
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