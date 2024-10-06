using Newtonsoft.Json;

namespace TongTongAdmin.Models
{
    public class Teacher
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("photoName")]
        public string PhotoName { get; set; }
    }
}