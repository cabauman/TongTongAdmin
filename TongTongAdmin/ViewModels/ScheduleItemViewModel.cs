using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TongTongAdmin.Common;
using TongTongAdmin.Models;

namespace TongTongAdmin.ViewModels
{
    public class ScheduleItemViewModel : INotifyPropertyChanged
    {
        public ScheduleItemViewModel() { }

        public ScheduleItemViewModel(List<Teacher> teacherPool)
        {
            Teachers = "N/A";
            Days = "N/A";
            FromTime = "N/A";
            ToTime = "N/A";
            Location = "N/A";

            InitTeacherOptions(teacherPool);
        }

        public string Id { get; set; }

        private string _teachers;
        public string Teachers
        {
            get { return _teachers; }
            set
            {
                _teachers = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Teachers)));
            }
        }

        private string _days;
        public string Days
        {
            get { return _days; }
            set
            {
                _days = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Days)));
            }
        }

        public string Location { get; set; }

        [JsonIgnore]
        public Dictionary<int, string> DayKeys { get; set; } // 0: d1 (Sunday)    -    1: d7 (Saturday)

        public event PropertyChangedEventHandler PropertyChanged;

        private string _fromTime;
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

        private string _toTime;
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

        public string TimeFrame
        {
            get { return string.Format("{0} - {1}", FromTime, ToTime); }
        }

        private SelectedDays _selectedDays;
        public SelectedDays SelectedDays
        {
            get { return _selectedDays; }
            set
            {
                _selectedDays = value;
                Days = _selectedDays.ToString().Replace(",", ", ");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDays)));
            }
        }

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

        public ObservableCollection<TeacherOption> TeacherOptions { get; set; }

        public void InitTeacherOptions(List<Teacher> teacherPool)
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
    }
}