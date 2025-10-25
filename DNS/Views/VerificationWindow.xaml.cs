using System;
using System.Windows;
using DNS.Services;

namespace DNS.Views
{
    public partial class VerificationWindow : Window
    {
        private readonly string _email;
        private readonly string _expectedCode;
        private readonly EmailService _emailService;
        private readonly Action _onSuccess;

        public VerificationWindow(string email, string code, Action onSuccess)
        {
            InitializeComponent();
            _email = email;
            _expectedCode = code;
            _emailService = new EmailService();
            _onSuccess = onSuccess;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var enteredCode = VerificationCodeBox.Text.Trim();
            
            if (string.IsNullOrEmpty(enteredCode))
            {
                MessageBox.Show("Пожалуйста, введите код подтверждения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (enteredCode == _expectedCode)
            {
                _onSuccess?.Invoke();
                Close();
            }
            else
            {
                MessageBox.Show("Неверный код подтверждения. Попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResendButton.IsEnabled = false;
                await _emailService.SendVerificationCodeAsync(_email, _expectedCode);
                MessageBox.Show("Код подтверждения отправлен повторно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки кода: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ResendButton.IsEnabled = true;
            }
        }
    }
}
