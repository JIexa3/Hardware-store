using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DNS.Models;
using DNS.Data;
using Microsoft.EntityFrameworkCore;

namespace DNS.Views
{
    public partial class OrderHistoryWindow : Window
    {
        private readonly User _currentUser;
        private readonly ApplicationDbContext _context;
        public ObservableCollection<Order> Orders { get; private set; }

        public OrderHistoryWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _context = new ApplicationDbContext();
            DataContext = this;
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var orders = _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.UserId == _currentUser.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                Orders = new ObservableCollection<Order>(orders);
                ordersListView.ItemsSource = Orders;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Order selectedOrder)
            {
                var detailsWindow = new OrderDetailsWindow(selectedOrder);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
