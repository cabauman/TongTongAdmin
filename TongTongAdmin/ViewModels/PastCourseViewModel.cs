using System.Collections.ObjectModel;
using TongTongAdmin.Models;

namespace TongTongAdmin.ViewModels
{
    public class PastCourseViewModel
    {
        public string CourseType { get; set; }

        public string Semester { get; set; }

        public ObservableCollection<SyllabusItem> SyllabusItems { get; set; }
    }
}