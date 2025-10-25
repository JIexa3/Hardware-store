using System.Windows;
using DNS.Data;
using DNS.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DNS.Views
{
    public partial class LoginWindow : Window
    {
        public User User { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                var user = context.Users
                    .FirstOrDefault(u => u.Username == username && u.Password == password);

                if (user != null)
                {
                    User = user;
                    
                    // Если пользователь - администратор
                    if (user.IsAdmin)
                    {
                        var adminWindow = new AdminWindow();
                        adminWindow.Show();
                        Application.Current.MainWindow.Hide();
                        Close();
                        
                        // При закрытии окна администратора
                        adminWindow.Closed += (s, args) =>
                        {
                            Application.Current.MainWindow.Show();
                        };
                    }
                    else
                    {
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Owner = this;
            if (registerWindow.ShowDialog() == true)
            {
                // После успешной регистрации заполняем поля для входа
                UsernameTextBox.Text = registerWindow.Username;
                PasswordBox.Password = registerWindow.Password;
            }
        }
    }
}
