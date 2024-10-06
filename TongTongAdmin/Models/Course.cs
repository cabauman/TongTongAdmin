using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace TongTongAdmin.Models
{
    public class Course : ICourse, INotifyPropertyChanged
    {
        private string _title;
        private string _studentCsv;
        private ObservableCollection<User> _studentUsers;

        public Course()
        {
        }

        public Course(Course other)
        {
            var dayToTimeMapCopy = new Dictionary<string, object>(other.DayToTimeMap);
            var studentsCopy = new Dictionary<string, string>(other.Students);

            Title = other.Title;
            CourseType = other.CourseType;
            Tuition = other.Tuition;
            ThisSemester = other.ThisSemester;
            DayToTimeMap = dayToTimeMapCopy;
            Students = studentsCopy;
            // We don't make a copy of the syllabus because that should be remade by the teacher.
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public bool DidInit { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("title")]
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        [JsonProperty("courseType")]
        public string CourseType { get; set; }

        [JsonProperty("tuition")]
        public string Tuition { get; set; }

        [JsonProperty("thisSemester")]
        public bool ThisSemester { get; set; }

        [JsonProperty("uidToAmountPaidDict")]
        public IDictionary<string, uint> UidToAmountPaidDict { get; set; }

        [JsonProperty("hourlyRate")]
        public uint HourlyRate{ get; set; }

        [JsonProperty("students")]
        public IDictionary<string, string> Students { get; set; }

        [JsonProperty("dayToTimeMap")]
        public IDictionary<string, object> DayToTimeMap { get; set; }

        [JsonIgnore]
        public ObservableCollection<SyllabusItem> SyllabusItems { get; set; }

        [JsonIgnore]
        public ObservableCollection<ScheduleItem> ScheduleItems { get; set; }

        [JsonIgnore]
        public SyllabusItem SelectedSyllabusItem { get; set; }

        [JsonIgnore]
        public ScheduleItem SelectedScheduleItem { get; set; }

        [JsonIgnore]
        public User SelectedStudent { get; set; }

        [JsonIgnore]
        public string Teachers
        {
            get { return ScheduleItems?.First()?.Teachers; }
        }

        [JsonIgnore]
        public ObservableCollection<User> StudentUsers
        {
            get { return _studentUsers; }
            set
            {
                _studentUsers = value;
                StudentCsv = value == null ? "No students" : string.Join(", ", value.Select(x => x.Name));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentUsers)));
            }
        }
        
        [JsonIgnore]
        public string StudentCsv
        {
            get { return _studentCsv; }
            set
            {
                _studentCsv = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentCsv)));
            }
        }

        public void RemoveSelectedStudent()
        {
            Students.Remove(SelectedStudent.Uid);
            StudentUsers.Remove(SelectedStudent);

            if(Students.Count == 0)
            {
                StudentCsv = "No Students";
            }
            else
            {
                StudentCsv = string.Join(", ", StudentUsers.Select(x => x.Name));
            }
        }

        public uint GetPaidTuition(string uid)
        {
            return UidToAmountPaidDict[uid];
        }
    }
}