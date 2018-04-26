using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TongTongAdmin.Models
{
    public class SyllabusItem
    {
        [JsonProperty("agenda")]
        public string Agenda { get; set; }

        [JsonProperty("homework")]
        public string Homework { get; set; }

        [JsonProperty("fromTime")]
        public string FromTime { get; set; }

        [JsonProperty("toTime")]
        public string ToTime { get; set; }
    }
}
