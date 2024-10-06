using System;
using Windows.UI.Xaml.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TongTongAdmin
{
    public class DateStringToDateTimeOffsetConverter : IValueConverter
    {
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:sszzz";

        private TimeSpan _timeObj;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(DateTimeOffset.TryParse((string)value, out DateTimeOffset result))
            {
                _timeObj = result.TimeOfDay;
                return result;
            }
            else
            {
                return DateTimeOffset.Now;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var date = (DateTimeOffset)value;
            date = date.Subtract(date.TimeOfDay);
            
            return date.Add(_timeObj).ToString(DATE_FORMAT);
        }
    }
}