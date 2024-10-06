using TongTongAdmin.Models;
using TongTongAdmin.Common;
using System.Collections.ObjectModel;
using System.Linq;

namespace TongTongAdmin.ViewModels
{
    public class CourseViewModel : BindableBase
    {
        private string _studentCsv;
        private ObservableCollection<User> _studentUsers;

        public CourseViewModel()
        {
        }

        public Course Model { get; }

        public string Title
        {
            get { return Model.Title; }
            set { Model.Title = value; }
        }

        public string Tuition
        {
            get { return Model.Tuition; }
            set { Model.Tuition = value; }
        }

        public bool ThisSemester
        {
            get { return Model.ThisSemester; }
            set { Model.ThisSemester = value; }
        }

        public ObservableCollection<SyllabusItem> SyllabusItems { get; set; }

        public ObservableCollection<ScheduleItem> ScheduleItems { get; set; }

        public ObservableCollection<User> StudentUsers
        {
            get { return _studentUsers; }
            set
            {
                StudentCsv = value == null ? "No students" : string.Join(", ", value.Select(x => x.Name));
                SetProperty(ref _studentUsers, value);
            }
        }

        public SyllabusItem SelectedSyllabusItem { get; set; }

        public ScheduleItem SelectedScheduleItem { get; set; }

        public User SelectedStudent { get; set; }

        public string StudentCsv
        {
            get { return _studentCsv; }
            set { SetProperty(ref _studentCsv, value); }
        }
    }
}