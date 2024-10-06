using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TongTongAdmin.Models
{
    public class CourseRegistration
    {
        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("registrantName")]
        public string RegistrantName { get; set; }

        [JsonProperty("courseTitle")]
        public string CourseType { get; set; }

        [JsonProperty("daysAvailable")]
        public string DaysAvailable { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("isConfirmed")]
        public bool IsConfirmed { get; set; }

        [JsonProperty("timestampMap")]
        public IDictionary<string, long> TimestampMap { get; set; }

        [JsonIgnore]
        public User Registrant { get; set; }
    }
}