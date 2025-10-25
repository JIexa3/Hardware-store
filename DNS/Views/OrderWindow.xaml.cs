using System.Windows;
using DNS.Models;
using DNS.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DNS.Views
{
    public partial class OrderWindow : Window
    {
        private readonly Order _order;

        public OrderWindow(Order order)
        {
            InitializeComponent();

            // Загружаем заказ со всеми связанными данными
            using (var context = new ApplicationDbContext())
            {
                _order = context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefault(o => o.Id == order.Id);
            }

            if (_order != null)
            {
                DataContext = _order;
                OrderItemsDataGrid.ItemsSource = _order.OrderItems;
            }
            else
            {
                MessageBox.Show("Не удалось загрузить данные заказа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
