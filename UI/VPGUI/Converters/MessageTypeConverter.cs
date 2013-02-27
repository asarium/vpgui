using System;
using System.Globalization;
using System.Windows.Data;
using VPSharp;

namespace VPGUI.Converters
{
    internal class MessageTypeConverter : IValueConverter
    {
        public object ErrorBrush { get; set; }
        public object WarningBrush { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageType)
            {
                switch ((MessageType) value)
                {
                    case MessageType.ERROR:
                        return this.ErrorBrush;
                    case MessageType.WARNING:
                        return this.WarningBrush;

                    default:
                        return null;
                }
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
