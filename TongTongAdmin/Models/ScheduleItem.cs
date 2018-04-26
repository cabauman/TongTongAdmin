using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TongTongAdmin.Models
{
    public class ScheduleItem
    {
        [JsonProperty("teachersCsv")]
        public string TeachersCsv { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("fromTime")]
        public string FromTime { get; set; }

        [JsonProperty("toTime")]
        public string ToTime { get; set; }
    }
}
