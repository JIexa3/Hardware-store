using System;
using System.Windows;
using DNS.Data;
using DNS.Models;
using DNS.Services;
using System.Linq;
using System.Text.RegularExpressions;

namespace DNS.Views
{
    public partial class RegisterWindow : System.Windows.Window
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public RegisterWindow()
        {
            InitializeComponent();
            _context = new ApplicationDbContext();
            _emailService = new EmailService();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UsernameBox.Text;
                string email = EmailBox.Text;
                string password = PasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;

                // Проверка заполнения полей
                if (string.IsNullOrWhiteSpace(username) || 
                    string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(password) || 
                    string.IsNullOrWhiteSpace(confirmPassword))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка email
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Пожалуйста, введите корректный email адрес.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка совпадения паролей
                if (password != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка существования пользователя
                if (_context.Users.Any(u => u.Username == username))
                {
                    MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_context.Users.Any(u => u.Email == email))
                {
                    MessageBox.Show("Пользователь с таким email уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создание нового пользователя
                var user = new User
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    IsAdmin = false,
                    IsEmailVerified = false,
                    RegisterDate = DateTime.Now
                };

                // Генерация кода подтверждения
                var verificationCode = GenerateVerificationCode();

                // Отправка кода подтверждения
                await _emailService.SendVerificationCodeAsync(user.Email, verificationCode);

                // Открытие окна подтверждения
                var verificationWindow = new VerificationWindow(user.Email, verificationCode, () =>
                {
                    user.IsEmailVerified = true;
                    _context.Users.Add(user);
                    _context.SaveChanges();

                    Username = username;
                    Password = password;

                    MessageBox.Show("Регистрация успешно завершена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                });

                verificationWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
