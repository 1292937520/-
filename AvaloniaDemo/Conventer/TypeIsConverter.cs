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
    public class TypeIsConverter : IValueConverter
    {
        // 单例模式
        public static readonly TypeIsConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // 获取参数类型名称
            var typeName = parameter is string strParam
                ? strParam
                : parameter.GetType().Name;

            // 比较类型名称（不带ViewModel后缀）
            var valueTypeName = value.GetType().Name.Replace("ViewModel", "");
            return valueTypeName.Equals(typeName.Replace("ViewModel", ""), StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
