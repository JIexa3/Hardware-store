using System.Windows;
using DNS.Models;
using System.Collections.Generic;

namespace DNS.Views
{
    public partial class OrderStatusDialog : Window
    {
        public OrderStatus SelectedStatus { get; private set; }

        public OrderStatusDialog(OrderStatus currentStatus)
        {
            InitializeComponent();
            SelectedStatus = currentStatus;

            var statuses = new List<OrderStatus>
            {
                OrderStatus.Новый,
                OrderStatus.Собирается,
                OrderStatus.Выполнен
            };

            StatusComboBox.ItemsSource = statuses;
            StatusComboBox.SelectedItem = currentStatus;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is OrderStatus status)
            {
                SelectedStatus = status;
                DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
