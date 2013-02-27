using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VPGUI.Converters
{
    public class WindowStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindowState)
            {
                switch ((WindowState) value)
                {
                    case WindowState.Normal:
                        return false;
                    case WindowState.Minimized:
                        return false;
                    case WindowState.Maximized:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }
            }
            else
            {
                return value;
            }
        }
    }
}