using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.Conventer
{
    public class StatusToBrushConverter : IValueConverter
    {
        // 单例模式
        public static readonly StatusToBrushConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CheckStatus status)
            {
                return status switch
                {
                    CheckStatus.Passed => Brushes.Green,
                    CheckStatus.Failed => Brushes.Red,
                    CheckStatus.Downloading => Brushes.Blue,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
