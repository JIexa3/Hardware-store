using System.Windows;

namespace DNS.Views
{
    public partial class TextDialog : Window
    {
        public string ResponseText { get; private set; }

        public TextDialog(string title, string prompt, string defaultText = "")
        {
            InitializeComponent();
            Title = title;
            PromptText.Text = prompt;
            ResponseTextBox.Text = defaultText;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = ResponseTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
