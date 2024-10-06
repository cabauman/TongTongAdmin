using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TongTongAdmin.Models
{
    public class TeacherOption : INotifyPropertyChanged
    {
        private bool _selected;

        public TeacherOption() { }

        public TeacherOption(TeacherOption other)
        {
            Name = other.Name;
            Selected = other.Selected;
        }

        public string Name { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }
    }
}