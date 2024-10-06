using System;
using Windows.UI.Xaml.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TongTongAdmin
{
    public class DateStringToTimeSpanConverter : IValueConverter
    {
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:sszzz";

        private DateTime _dateObj;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(DateTime.TryParse((string)value, out _dateObj))
            {
                return _dateObj.TimeOfDay;
            }
            else
            {
                return DateTime.Now.TimeOfDay;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var time = (TimeSpan)value;
            _dateObj = _dateObj.Subtract(_dateObj.TimeOfDay);
            
            return _dateObj.Add(time).ToString(DATE_FORMAT);
        }
    }
}