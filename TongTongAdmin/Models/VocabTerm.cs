using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TongTongAdmin.Models
{
    public class VocabTerm
    {
        public string Id { get; set; }

        public string Ko { get; set; }

        public string Translation { get; set; }

        public string Romanization { get; set; }

        public string ExtraInfoId { get; set; }

        public List<string> SentenceIds { get; set; }
        
        public List<string> ImageIds { get; set; }
    }
}