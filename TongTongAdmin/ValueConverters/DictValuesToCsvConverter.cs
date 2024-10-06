using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TongTongAdmin.Models;
using Windows.UI.Xaml.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TongTongAdmin
{
    public class DictValuesToCsvConverter : IValueConverter
    {
        private const string DATE_FORMAT = "dddd, MMMM dd, h:mm tt";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //return value != null ? string.Join(", ", ((IDictionary<string, string>)value).Values) : value;
            return value == null ? "No students" : string.Join(", ", ((ObservableCollection<User>)value).Select(x => x.Name));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}