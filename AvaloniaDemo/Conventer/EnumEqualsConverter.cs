using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.Conventer
{
    public class EnumEqualsConverter : IValueConverter
    {
        public static readonly EnumEqualsConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var valueStr = value.ToString();
            var parameterStr = parameter.ToString();
            return valueStr.Equals(parameterStr, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
