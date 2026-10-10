using MessengerClient.Pages;
using MessengerClient.Services;
using System.Windows;

namespace MessengerClient.Windows
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Client _client;
        public MainWindow()
        {
            InitializeComponent();

            _client = new Client();
            MainFrame.Navigate(new LoginPage(_client));
        }


    }
}
