using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TongTongAdmin.Models
{
    public class SyllabusItem : INotifyPropertyChanged
    {
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:sszzz";
        private const string SEMESTER_FORMAT = "yyyy년 MM월";

        private string _fromTime;
        private string _toTime;

        public SyllabusItem()
        {
            FromTime = DateTime.Now.ToString(DATE_FORMAT);
            ToTime = DateTime.Now.ToString(DATE_FORMAT);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("agenda")]
        public string Agenda { get; set; }

        [JsonProperty("homework")]
        public string Homework { get; set; }

        [JsonProperty("fromTime")]
        public string FromTime // Formatted date: 2017-11-04 10:00:19+0900
        {
            get { return _fromTime; }
            set
            {
                _fromTime = value;

                if(DateTimeOffset.TryParse(value, out DateTimeOffset fromDateTime))
                {
                    if(DateTimeOffset.TryParse(ToTime, out DateTimeOffset toDateTime))
                    {
                        var toTimeOfDay = toDateTime.TimeOfDay;
                        var modifiedDate = fromDateTime.Date;
                        modifiedDate = modifiedDate.Subtract(modifiedDate.TimeOfDay);
                        ToTime = modifiedDate.Add(toTimeOfDay).ToString(DATE_FORMAT);

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FromTime)));
                    }
                }
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
            }
        }

        [JsonIgnore]
        public float ClassDuration
        {
            get
            {
                if(DateTimeOffset.TryParse(_fromTime, out DateTimeOffset fromDateTime))
                {
                    if(DateTimeOffset.TryParse(_toTime, out DateTimeOffset toDateTime))
                    {
                        var timeSpan = toDateTime - fromDateTime;

                        return (float)timeSpan.TotalHours;
                    }
                }

                // Just return 1 hour as a default.
                return 1;
            }
        }

        [JsonIgnore]
        public DateTime FromDate
        {
            get
            {
                if(DateTime.TryParse(_fromTime, out DateTime fromDateTime))
                {
                    return fromDateTime;
                }

                return DateTime.Now;
            }
        }

        [JsonIgnore]
        public string Semester
        {
            get
            {
                DateTimeOffset date = DateTimeOffset.Parse(FromTime);
                return date.ToString(SEMESTER_FORMAT);
            }
        }
    }
}