using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TongTongAdmin.Common;

namespace TongTongAdmin.ViewModels.Pages
{
    public class CourseListPageViewModel : BindableBase
    {
        private CourseViewModel _selectedCourse;

        public ObservableCollection<CourseViewModel> Courses { get; set; }

        public CourseViewModel SelectedCourse
        {
            get { return _selectedCourse; }
            set { SetProperty(ref _selectedCourse, value); }
        }
    }
}