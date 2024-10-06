using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TongTongAdmin
{
    public class DateStringToFormattedStringConverter : IValueConverter
    {
        private const string DATE_FORMAT = "dddd, MMMM dd, h:mm tt";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string dateFormat = parameter as string;

            if(DateTime.TryParse((string)value, out DateTime result))
            {
                return result.ToString(dateFormat);
            }
            else
            {
                return DateTime.Now;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}