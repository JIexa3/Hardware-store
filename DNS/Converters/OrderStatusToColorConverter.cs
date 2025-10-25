using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DNS.Models;

namespace DNS.Converters
{
    public class OrderStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.Новый => new SolidColorBrush(Colors.Blue),
                    OrderStatus.Собирается => new SolidColorBrush(Colors.Orange),
                    OrderStatus.Выполнен => new SolidColorBrush(Colors.Green),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
