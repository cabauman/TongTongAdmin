using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TongTongAdmin
{
    public class TimeStringToTimeSpanConverter : IValueConverter
    {
        private const string TIME_FORMAT = "h:mm tt";
        private static readonly CultureInfo CI = new CultureInfo("en-US");

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime dateTime = DateTime.ParseExact((string)value, TIME_FORMAT, CI);
            return dateTime.TimeOfDay;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            DateTime dateTime = DateTime.Today.Add((TimeSpan)value);
            return dateTime.ToString(TIME_FORMAT, CI);
        }
    }
}