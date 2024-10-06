using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TongTongAdmin.Common;

namespace TongTongAdmin.Models
{
    public class ScheduleItem : INotifyPropertyChanged
    {
        private string _teachers;
        private string _days;
        private string _fromTime;
        private string _toTime;
        private SelectedDays _selectedDays;

        public ScheduleItem() { }

        public ScheduleItem(IReadOnlyList<Teacher> teacherPool)
        {
            Teachers = "N/A";
            Days = "N/A";
            FromTime = "N/A";
            ToTime = "N/A";
            Location = "N/A";

            InitTeacherOptions(teacherPool);
        }

        public ScheduleItem(ScheduleItem other)
        {
            Id = other.Id;
            FromTime = other.FromTime;
            ToTime = other.ToTime;
            SelectedDays = other.SelectedDays;
            Days = other.Days;
            DayKeys = new List<string>(other.DayKeys);
            Location = other.Location;
            Teachers = other.Teachers;
            TeacherOptions = new ObservableCollection<TeacherOption>(other.TeacherOptions.Select(x => new TeacherOption()));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("teachers")]
        public string Teachers
        {
            get { return _teachers; }
            set
            {
                _teachers = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Teachers)));
            }
        }

        [JsonProperty("days")]
        public string Days
        {
            get { return _days; }
            set
            {
                _days = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Days)));
            }
        }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("dayKeys")]
        public List<string> DayKeys { get; set; } // 0: d1 (Sunday)    -    1: d7 (Saturday)

        [JsonProperty("fromTime")]
        public string FromTime // 11:00 AM
        {
            get { return _fromTime; }
            set
            {
                _fromTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FromTime)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeFrame)));
            }
        }

        [JsonProperty("toTime")]
        public string ToTime
        {
            get { return _toTime; }
            set
            {
                _toTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToTime)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeFrame)));
            }
        }

        [JsonIgnore]
        public string TimeFrame
        {
            get { return string.Format("{0} - {1}", FromTime, ToTime); }
        }

        [JsonIgnore]
        public SelectedDays LastModifiedDay { get; private set; }

        [JsonIgnore]
        public SelectedDays SelectedDays
        {
            get { return _selectedDays; }
            set
            {
                LastModifiedDay = (SelectedDays)Math.Abs(value - _selectedDays);
                _selectedDays = value;
                Days = _selectedDays.ToString();
                
                DayKeys = DayValues
                    .Where(day => _selectedDays.HasFlag(day))
                    .Select(day => DayFlagToDayKeyMap[day])
                    .ToList();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDays)));
            }
        }

        [JsonIgnore]
        public ObservableCollection<TeacherOption> TeacherOptions { get; set; }

        public void InitDays()
        {
            _selectedDays = Days.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    Enum.TryParse(x, out SelectedDays result);
                    return result;
                })
                .Aggregate((x, y) => x |= y);
        }
        
        public void InitTeacherOptions(IReadOnlyList<Teacher> teacherPool)
        {
            var selectedTeachers = Teachers.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            var options = teacherPool.Select(teacher =>
                new TeacherOption
                {
                    Name = teacher.Name,
                    Selected = selectedTeachers.Contains(teacher.Name)
                });

            TeacherOptions = new ObservableCollection<TeacherOption>(options);

            foreach(var option in TeacherOptions)
            {
                option.PropertyChanged += (s, e) =>
                {
                    Teachers = string.Join(", ", TeacherOptions.Where(x => x.Selected).Select(x => x.Name));
                };
            }
        }


        public static IEnumerable<SelectedDays> DayValues = Enum.GetValues(typeof(SelectedDays)).Cast<SelectedDays>();

        public static Dictionary<SelectedDays, string> DayFlagToDayKeyMap = new Dictionary<SelectedDays, string>()
        {
            { SelectedDays.Mo, "d2" },
            { SelectedDays.Tu, "d3" },
            { SelectedDays.We, "d4" },
            { SelectedDays.Th, "d5" },
            { SelectedDays.Fr, "d6" },
            { SelectedDays.Sa, "d7" },
        };
    }
}