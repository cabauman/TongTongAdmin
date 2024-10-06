using System;
using TongTongAdmin.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TongTongAdmin
{
    public class EnumToBoolConverter : IValueConverter
    {
        private int _flags = 0;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value == null) return false;
            _flags = (int)value;
            Type t = value.GetType();
            object o = Enum.ToObject(t, parameter);

            return ((Enum)value).HasFlag((Enum)o);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if(value?.Equals(true) ?? false)
            {
                _flags = _flags | (int)parameter;
            }
            else
            {
                _flags = _flags & ~(int)parameter;
            }

            return _flags;
        }
    }
}