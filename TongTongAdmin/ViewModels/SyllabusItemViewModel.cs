using System;
using System.ComponentModel;

namespace TongTongAdmin.ViewModels
{
    public class SyllabusItemViewModel : INotifyPropertyChanged
    {
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:sszzz";

        private string _fromTime;
        private string _toTime;

        public SyllabusItemViewModel()
        {
            FromTime = DateTime.Now.ToString(DATE_FORMAT);
            ToTime = DateTime.Now.ToString(DATE_FORMAT);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Id { get; set; }

        public string Agenda { get; set; }

        public string Homework { get; set; }
        
        public string FromTime // Formatted date: 2017-11-04 10:00:19+0900
        {
            get { return _fromTime; }
            set
            {
                _fromTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FromTime)));
            }
        }
        
        public string ToTime
        {
            get { return _toTime; }
            set
            {
                _toTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToTime)));
            }
        }
    }
}