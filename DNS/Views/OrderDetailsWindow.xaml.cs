using System.Windows;
using DNS.Models;
using System.Windows.Media;

namespace DNS.Views
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly Order _order;

        public OrderDetailsWindow(Order order)
        {
            InitializeComponent();
            _order = order;
            LoadOrderDetails();
            DataContext = _order;

            // Устанавливаем цвет статуса
            var statusBrush = _order.Status switch
            {
                OrderStatus.Новый => new SolidColorBrush(Colors.Blue),
                OrderStatus.Собирается => new SolidColorBrush(Colors.Orange),
                OrderStatus.Выполнен => new SolidColorBrush(Colors.Green),
                _ => new SolidColorBrush(Colors.Gray)
            };
            orderStatusTextBlock.Foreground = statusBrush;
        }

        private void LoadOrderDetails()
        {
            if (_order != null)
            {
                orderDateTextBlock.Text = $"Дата заказа: {_order.OrderDate:dd.MM.yyyy HH:mm}";
                orderStatusTextBlock.Text = $"Статус: {GetStatusText(_order.Status)}";
                orderTotalTextBlock.Text = $"Сумма: {_order.TotalAmount:N0} ₽";
                orderItemsListView.ItemsSource = _order.OrderItems;
            }
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Новый => "Новый",
                OrderStatus.Собирается => "Собирается",
                OrderStatus.Выполнен => "Выполнен",
                _ => "Неизвестно"
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
