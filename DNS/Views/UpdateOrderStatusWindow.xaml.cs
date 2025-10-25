using System;
using System.Windows;
using DNS.Models;
using DNS.Data;
using System.Collections.Generic;

namespace DNS.Views
{
    public partial class UpdateOrderStatusWindow : Window
    {
        private readonly Order _order;

        public UpdateOrderStatusWindow(Order order)
        {
            InitializeComponent();
            _order = order;

            var statuses = new List<OrderStatus>
            {
                OrderStatus.Новый,
                OrderStatus.Собирается,
                OrderStatus.Выполнен
            };

            StatusComboBox.ItemsSource = statuses;
            StatusComboBox.SelectedItem = _order.Status;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is OrderStatus newStatus)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var order = context.Orders.Find(_order.Id);
                        if (order != null)
                        {
                            order.Status = newStatus;
                            context.SaveChanges();
                            DialogResult = true;
                            Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
