using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace TongTongAdmin.Helpers
{
    public static class BrushHelper
    {
        public static Brush PickRandomDarkBrush()
        {
            Color result = Colors.AliceBlue;
            Random rnd = new Random();
            Type brushesType = typeof(Colors);
            PropertyInfo[] properties = brushesType.GetProperties(BindingFlags.Public | BindingFlags.Static);
            int random = rnd.Next(properties.Length);
            result = (Color)properties[random].GetValue(null, null);

            while(!ColorIsDark(result))
            {
                random = rnd.Next(properties.Length);
                result = (Color)properties[random].GetValue(null, null);
            }

            return new SolidColorBrush(result);
        }

        public static bool ColorIsDark(Color color)
        {
            bool colorIsDark = (2 * color.R + 5 * color.G + color.B) <= 10 * 128;

            return colorIsDark;
        }
    }
}