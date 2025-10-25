using System;
using System.Globalization;
using System.Windows.Data;
using DNS.Models;

namespace DNS.Converters
{
    public class OrderStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                return status.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string statusString)
            {
                if (Enum.TryParse<OrderStatus>(statusString, out var status))
                {
                    return status;
                }
            }
            return OrderStatus.Новый;
        }
    }
}
