using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq;
using TongTongAdmin.Helpers;
using Windows.UI.Xaml.Media;

namespace TongTongAdmin.ViewModels
{
    public class UserViewModel : INotifyPropertyChanged
    {
        public string Uid { get; set; }

        private string _name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public string Username { get; set; }

        public string PhoneNum { get; set; }

        public string Email { get; set; }

        public string SkypeId { get; set; }

        public string PhotoUrl { get; set; }

        public int StudyPoints { get; set; }

        public bool IsAdmin { get; set; }

        public string InstanceIdToken { get; set; }

        // ------------------------------------------

        [JsonIgnore]
        public bool PhotoUrlIsNull
        {
            get { return string.IsNullOrEmpty(PhotoUrl); }
        }

        [JsonIgnore]
        public string Initials
        {
            get
            {
                return Name.Split(' ')
                    .Take(2)
                    .Aggregate((first, last) =>
                        first.Substring(0, 1) + last.Substring(0, 1))
                    .ToUpper();
            }
        }

        [JsonIgnore]
        public Brush InitialsFillBrush
        {
            get { return BrushHelper.PickRandomDarkBrush(); }
        }

        public string CourseId { get; set; }

        public string CourseType { get; set; }

        public string NextSemesterCourseId { get; set; }

        public string NextSemesterCourseType { get; set; }

        public bool DidPayTuition { get; set; }

        public bool DidRegister { get; set; }

        public bool HasCurrentOrUpcomingCourse { get; set; }

        public string Notes { get; set; }
    }
}