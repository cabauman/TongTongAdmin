using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TongTongAdmin.Models
{
    public class VocabSectionChild
    {
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("iconId")]
        public string IconId { get; set; }

        // This will go in the ViewModel
        //public bool IsSubsection { get; }

        [JsonProperty("study-sets")]
        public Dictionary<string, object> StudySets { get; set; }
    }
}