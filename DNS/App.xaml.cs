using System.Windows;
using DNS.Views;
using DNS.Data;

namespace DNS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize database
            using (var context = new ApplicationDbContext())
            {
                context.Database.EnsureCreated();
                DbInitializer.Initialize(context);
            }

            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
