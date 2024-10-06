using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TongTongAdmin.Common;

namespace TongTongAdmin.ViewModels.Pages
{
    public class StudentListPageViewModel : BindableBase
    {
        private UserViewModel _selectedStudent;

        public ObservableCollection<CourseViewModel> Students { get; set; }

        public UserViewModel SelectedStudent
        {
            get { return _selectedStudent; }
            set { SetProperty(ref _selectedStudent, value); }
        }
    }
}