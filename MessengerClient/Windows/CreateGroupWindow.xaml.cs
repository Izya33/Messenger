using MessengerClient.Services;
using MessengerShared.Communication;
using MessengerShared.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace MessengerClient.Windows
{
    /// <summary>
    /// Interaktionslogik für CreateGroupWindow.xaml
    /// </summary>
    public partial class CreateGroupWindow : Window
    {
        private readonly Client _client;

        public CreateGroupWindow(Client client, List<FriendsInfo> friends)
        {
            InitializeComponent();

            _client = client;
            FriendsList.ItemsSource = friends;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GroupNameBox.Text)) return;

            List<int> memberIds = new();

            foreach (FriendsInfo friend in FriendsList.Items)
            {
                ListBoxItem? item = FriendsList.ItemContainerGenerator.ContainerFromItem(friend)  as ListBoxItem;

                if (item == null) continue;

                CheckBox? checkBox = FindVisualChild<CheckBox>(item);

                if (checkBox?.IsChecked == true) memberIds.Add(friend.Id);
            }

            // later send CreateGroup
            _client.CreateGroup(GroupNameBox.Text, memberIds);

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result) return result;

                T? nested = FindVisualChild<T>(child);

                if (nested != null) return nested;
            }

            return null;
        }
    }
}
