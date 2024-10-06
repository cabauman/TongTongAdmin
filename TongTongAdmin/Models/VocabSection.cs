using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TongTongAdmin.Models
{
    public class VocabSection
    {
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("colorTheme")]
        public string ColorTheme { get; set; }

        [JsonProperty("order")]
        public string Order { get; set; }

        [JsonProperty("study-sets")]
        public Dictionary<string, object> StudySets { get; set; }

        [JsonProperty("subsections")]
        public Dictionary<string, object> Subsections { get; set; }

        // This will go in the ViewModel
        //public List<VocabSectionChild> Children { get; set; }
    }
}