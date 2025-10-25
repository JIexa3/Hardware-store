using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DNS.Models;
using DNS.Data;
using Microsoft.EntityFrameworkCore;

namespace DNS.Views
{
    public partial class ProfileWindow : Window, INotifyPropertyChanged
    {
        private readonly User _currentUser;
        private string _username;
        private string _email;
        private DateTime _registrationDate;
        private int _orderCount;
        private decimal _totalOrderAmount;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public DateTime RegistrationDate
        {
            get => _registrationDate;
            set
            {
                _registrationDate = value;
                OnPropertyChanged(nameof(RegistrationDate));
            }
        }

        public int OrderCount
        {
            get => _orderCount;
            set
            {
                _orderCount = value;
                OnPropertyChanged(nameof(OrderCount));
            }
        }

        public decimal TotalOrderAmount
        {
            get => _totalOrderAmount;
            set
            {
                _totalOrderAmount = value;
                OnPropertyChanged(nameof(TotalOrderAmount));
            }
        }

        public ProfileWindow(User currentUser)
        {
            InitializeComponent();
            DataContext = this;
            _currentUser = currentUser;

            if (currentUser != null)
            {
                Username = currentUser.Username;
                Email = currentUser.Email;
                RegistrationDate = currentUser.RegisterDate;

                LoadUserStatistics();
            }
        }

        private void LoadUserStatistics()
        {
            using (var context = new ApplicationDbContext())
            {
                // Получаем количество заказов
                OrderCount = context.Orders.Count(o => o.UserId == _currentUser.Id);

                // Получаем общую сумму заказов
                var totalAmount = context.Orders
                    .Where(o => o.UserId == _currentUser.Id)
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                TotalOrderAmount = totalAmount;
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Новые пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                var user = context.Users.Find(_currentUser.Id);
                if (user != null)
                {
                    if (CurrentPasswordBox.Password != user.Password)
                    {
                        MessageBox.Show("Неверный текущий пароль", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Обновляем пароль
                    user.Password = NewPasswordBox.Password;
                    context.SaveChanges();

                    MessageBox.Show("Пароль успешно изменен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Очищаем поля
                    CurrentPasswordBox.Clear();
                    NewPasswordBox.Clear();
                    ConfirmPasswordBox.Clear();
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DialogResult = true;
                Close();
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
