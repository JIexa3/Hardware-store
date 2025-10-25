using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DNS.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _fromEmail = "storedn238@gmail.com"; // Замените на ваш email
        private readonly string _password = "uyss sxrm yluq wmoz"; // Замените на пароль приложения Gmail

        public async Task SendVerificationCodeAsync(string toEmail, string code)
        {
            try
            {
                var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_fromEmail, _password)
                };

                var message = new MailMessage(
                    from: _fromEmail,
                    to: toEmail,
                    subject: "DNS - Подтверждение регистрации",
                    body: $"Ваш код подтверждения: {code}\n\nВведите этот код для завершения регистрации."
                );

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка отправки email: {ex.Message}");
            }
        }
    }
}
