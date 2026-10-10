using MessengerClient.Services;
using MessengerShared.Communication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MessengerClient.Pages
{
    /// <summary>
    /// Interaktionslogik für LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly Client _client;
        public LoginPage(Client client)
        {
            InitializeComponent();

            _client = client;
            _client.LoginSucceeded += OnLoginSucceeded;
            _client.RegisterSucceeded += OnRegisterSucceeded;
            _client.ErrorReceived += OnError;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _client.Connect("127.0.0.1", 13000);

            _client.Sender = UsernameBox.Text;
            _client.SendMessage(MessageType.Login, PassworldBox.Password);

        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if(UsernameBox.Text.Length < 4)
            {
                Info.Text = "Error: Minimal Length of Username is 5. ";
            }
            else
            {
                if(PassworldBox.Password.Length < 3)
                {
                    Info.Text = "Error: Minimal Length of Passworld is 4. ";
                }
                else
                {
                    _client.Connect("127.0.0.1", 13000);

                    _client.Sender = UsernameBox.Text;
                    _client.SendMessage(MessageType.Register, PassworldBox.Password);
                }
            }

        }

        private void OnLoginSucceeded()
        {
            Dispatcher.Invoke(() =>
            {
                _client.LoginSucceeded -= OnLoginSucceeded;
                _client.RegisterSucceeded -= OnRegisterSucceeded;
                _client.ErrorReceived -= OnError;
                NavigationService.Navigate(new MessengerPage(_client));
            });
        }

        private void OnRegisterSucceeded()
        {
            Dispatcher.Invoke(() =>
            {
                Info.Text = "Info: You succesfully registred! \nNow you can login. ";
            });
        }

        private void OnError(String error)
        {
            Dispatcher.Invoke(() =>
            {
                Info.Text = "Error: "+ error;
            });
        }
    }
}
