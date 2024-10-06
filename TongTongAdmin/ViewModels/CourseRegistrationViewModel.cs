using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TongTongAdmin.Models;

namespace TongTongAdmin.ViewModels
{
    public class CourseRegistrationViewModel
    {
        public string Id { get; set; }

        public string RegistrantName { get; set; }

        public string CourseTitle { get; set; }

        public string DaysAvailable { get; set; }

        public string Location { get; set; }

        public bool IsConfirmed { get; set; }

        public User Registrant { get; set; }
    }
}