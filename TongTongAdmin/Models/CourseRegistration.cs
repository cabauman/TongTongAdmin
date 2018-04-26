using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TongTongAdmin.Models
{
    public class CourseRegistration
    {
        [JsonProperty("registrantName")]
        public string RegistrantName { get; set; }

        [JsonProperty("courseType")]
        public string CourseType { get; set; }

        [JsonProperty("daysAvailable")]
        public string DaysAvailable { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("isConfirmed")]
        public bool IsConfirmed { get; set; }

        [JsonProperty("timestampMap")]
        public IDictionary<string, long> TimestampMap { get; set; }
    }
}
