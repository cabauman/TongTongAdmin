using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TongTongAdmin.Models
{
    public class Syllabus
    {
        public string Id { get; set; }

        public ObservableCollection<SyllabusItem> SyllabusItems { get; set; }
    }
}