using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TongTongAdmin.Models
{
    public class PastCourse : ICourse
    {
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:sszzz";

        private ObservableCollection<SyllabusItem> _syllabusItems;

        public PastCourse()
        {
        }

        public PastCourse(Course course, ObservableCollection<SyllabusItem> syllabusItems)
        {
            CourseType = course.CourseType;
            HourlyRate = course.HourlyRate;
            Teachers = course.ScheduleItems?.First()?.Teachers;
            StudentUsers = new ObservableCollection<User>(course.StudentUsers);

            var nonEmptySyllabusItems = syllabusItems
                ?.Where(item => !string.IsNullOrEmpty(item.Agenda) || !string.IsNullOrEmpty(item.Homework))
                .ToDictionary(x => x.Id);

            // Make sure we have at least one syllabus item, regardless of
            // whether agenda and homework fields are empty.
            if(nonEmptySyllabusItems.Count() == 0 && syllabusItems?.Count > 0)
            {
                nonEmptySyllabusItems.Add(syllabusItems[0].Id, syllabusItems[0]);
            }

            string semester = syllabusItems?.Count > 0 ? syllabusItems[0].FromTime : DateTimeOffset.Now.AddMonths(-1).ToString(DATE_FORMAT);
        }

        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("courseType")]
        public string CourseType { get; set; }

        // "yyyy-MM-dd HH:mm:ssZ"
        [JsonProperty("semester")]
        public string Semester { get; set; }

        [JsonProperty("syllabus")]
        public IDictionary<string, SyllabusItem> Syllabus { get; set; }
        
        [JsonProperty("amountPaid")]
        public uint AmountPaid { get; set; }

        [JsonProperty("hourlyRate")]
        public uint HourlyRate { get; set; }

        [JsonIgnore]
        public string Uid { get; set; }

        [JsonIgnore]
        public ObservableCollection<SyllabusItem> SyllabusItems
        {
            get
            {
                if(_syllabusItems == null)
                {
                    _syllabusItems = new ObservableCollection<SyllabusItem>(Syllabus.Values);
                }

                return _syllabusItems;
            }
        }

        [JsonIgnore]
        public string Teachers { get; set; }

        [JsonIgnore]
        public ObservableCollection<User> StudentUsers { get; set; }

        public uint GetPaidTuition(string uid)
        {
            return AmountPaid;
        }
    }
}