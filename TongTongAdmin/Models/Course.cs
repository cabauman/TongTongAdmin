using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TongTongAdmin.Models
{
    public class Course
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("courseType")]
        public string CourseType { get; set; }

        [JsonProperty("hourlyRate")]
        public uint HourlyRate { get; set; }

        [JsonProperty("uidToAmountPaidDict")]
        public IDictionary<string, uint> UidToAmountPaidDict { get; set; }

        [JsonProperty("uidToStudentNameDict")]
        public IDictionary<string, string> UidToStudentNameDict { get; set; }
    }
}
