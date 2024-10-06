using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TongTongAdmin.Common;

namespace TongTongAdmin.ViewModels.Pages
{
    public class CourseRegistrationListPageViewModel : BindableBase
    {
        private CourseRegistrationViewModel _selectedRegistration;

        public ObservableCollection<CourseRegistrationViewModel> Registrations { get; set; }

        public CourseRegistrationViewModel SelectedRegistration
        {
            get { return _selectedRegistration; }
            set { SetProperty(ref _selectedRegistration, value); }
        }
    }
}