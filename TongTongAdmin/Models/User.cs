using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TongTongAdmin.Models
{
    public class User
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("phoneNum")]
        public string PhoneNum { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("skypeId")]
        public string SkypeId { get; set; }

        [JsonProperty("photoUrl")]
        public string PhotoUrl { get; set; }

        [JsonProperty("studyPoints")]
        public int StudyPoints { get; set; }

        [JsonProperty("instanceIdToken")]
        public string InstanceIdToken { get; set; }
    }
}
