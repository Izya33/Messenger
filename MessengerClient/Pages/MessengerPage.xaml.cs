using MessengerClient.Services;
using MessengerClient.Windows;
using MessengerShared.Communication;
using MessengerShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaktionslogik für MessengerPage.xaml
    /// </summary>
    public partial class MessengerPage : Page
    {
        private readonly Client _client;
        private List<DialogInfo> _dialogs = new();
        private List<FriendsInfo> _friends = new();
        private List<FriendRequestInfo> _requests = new();
        private int? _dialogToOpen;


        public MessengerPage(Client client)
        {
            InitializeComponent();
            _client = client;

            _client.MessageReceived += ReceiveMessage;
            _client.UpdateDialog += UpdateDialogs;
            _client.UpdateRequestList += UpdateRequestList;
            _client.UserFound += SetUser;
            _client.DialogCreated += OnDialogCreated;
            _client.FriendRequestSent += OnFriendRequestSent;
            _client.RequestReceived += OnRequestReceived;
            _client.RequestAccepted += OnRequestAccepted;
            _client.RequestDenied += OnRequestDenied;
            _client.UpdateFriends += UpdateFriends;
            _client.GroupCreated += OnGroupCreated;
        }


        // Client events
        private void ReceiveMessage(Message message)
        {
            if (message.DialogId != _client.DialogId) return;

            Dispatcher.Invoke(() =>
            {
                MessagesList.Items.Add( message.Sender + ": " + message.Content);
            });
        }

        private void UpdateDialogs(List<DialogInfo> dialogs)
        {
            _dialogs = dialogs;

            Dispatcher.Invoke(() =>
            {
                UpdateAllDialogs();
                UpdateFriendsDialogs();
                UpdateGroupDialogs();

                if (_dialogToOpen != null)
                {
                    DialogInfo? dialog = dialogs.FirstOrDefault( d => d.Id == _dialogToOpen.Value);

                    if (dialog != null)
                        OpenDialog(dialog);

                    _dialogToOpen = null;
                }
            });
        }
        private void UpdateRequestList(List<FriendRequestInfo> requests)
        {
            _requests = requests;

            Dispatcher.Invoke(() => 
            {
                RequestList.Items.Clear();
                
                foreach(FriendRequestInfo request in requests)
                    RequestList.Items.Add(request);

            });
        }

        public void SetUser(string username, FriendState state)
        {
            Dispatcher.Invoke(() =>
            {
                _client.Receiver = username;

                FoundUsername.Text = username;
                FoundUserPanel.Visibility = Visibility.Visible;

                SetFriendState(state);
            });
        }

        private void OnDialogCreated(int dialogId)
        {
            _dialogToOpen = dialogId;
            _client.SendMessage(MessageType.GetDialogs, null);

        }
        private void OnFriendRequestSent()
        {
            Dispatcher.Invoke(() =>
            {
                SetFriendState(FriendState.Pending);
            });
        }

        private void OnRequestReceived()
        {
            _client.SendMessage(MessageType.GetFriendRequests, null);
        }

        private void OnRequestAccepted()
        {
            Dispatcher.Invoke(() =>
            {
                SetFriendState(FriendState.Friend);
                _client.SendMessage(MessageType.GetFriends, null);
            });
        }

        private void OnRequestDenied()
        {
            Dispatcher.Invoke(() =>
            {
                SetFriendState(FriendState.None);
            });
        }

        private void UpdateFriends(List<FriendsInfo> friends)
        {
            _friends = friends;
            UpdateFriendsDialogs();
        }

        private void OnGroupCreated(int dialogId)
        {
            _client.SendMessage(MessageType.GetDialogs, null);
        }


        // Helpers
        private void OpenDialog(DialogInfo dialog)
        {
            MessagesList.Items.Clear();

            AllDialogsList.UnselectAll();
            FriendsDialogsList.UnselectAll();
            GroupDialogsList.UnselectAll();

            _client.DialogId = dialog.Id;
            _client.SendMessage(MessageType.GetMessages, null);
        }

        private void SetFriendState(FriendState state)
        {
            switch (state)
            {
                case FriendState.None:
                    AddFriendButton.Content = "Add";
                    AddFriendButton.IsEnabled = true;
                    break;

                case FriendState.Pending:
                    AddFriendButton.Content = "Pending";
                    AddFriendButton.IsEnabled = false;
                    break;

                case FriendState.Friend:
                    AddFriendButton.Content = "Friend";
                    AddFriendButton.IsEnabled = false;
                    break;
            }
        }

        private void UpdateAllDialogs()
        {
            Dispatcher.Invoke(() =>
            {
                AllDialogsList.Items.Clear();
                foreach (DialogInfo dialog in _dialogs)
                    AllDialogsList.Items.Add(dialog);
            });
        }

        private void UpdateFriendsDialogs()
        {
            Dispatcher.Invoke(() =>
            {
                FriendsDialogsList.Items.Clear();

                foreach (DialogInfo dialog in _dialogs)
                    foreach(FriendsInfo friend in _friends)
                    {
                        if (dialog.Type == DialogType.Private && dialog.OtherUserId == friend.Id)
                        {
                            FriendsDialogsList.Items.Add(dialog);
                        }
                            
                    }
            });
        }

        private void UpdateGroupDialogs()
        {
            Dispatcher.Invoke(() =>
            {
                GroupDialogsList.Items.Clear();

                foreach (DialogInfo dialog in _dialogs)
                    if(dialog.Type == DialogType.Group)
                        GroupDialogsList.Items.Add(dialog);
            });
        }


        // Dialog UI
        private void AllDialogsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AllDialogsList.SelectedItem is DialogInfo dialog)
                OpenDialog(dialog);
        }

        private void FriendsDialogsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FriendsDialogsList.SelectedItem is DialogInfo dialog)
                OpenDialog(dialog);
        }

        private void GroupDialogsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupDialogsList.SelectedItem is DialogInfo dialog)
                OpenDialog(dialog);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _client.SendMessage(MessageType.Message, InputMessage.Text);
            InputMessage.Clear();
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            CreateGroupWindow window = new CreateGroupWindow(_client, _friends);

            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }


        // User search / friends UI
        private void SearchUserButton_Click(object sender, RoutedEventArgs e)
        {
            FoundUserPanel.Visibility = Visibility.Collapsed;
            _client.SendMessage(MessageType.FindUser, UserSearchBox.Text);
            UserSearchBox.Clear();
        }

        private void OpenUserChat_Click(object sender, RoutedEventArgs e)
        {
            FoundUserPanel.Visibility = Visibility.Collapsed;

            _client.SendMessage(MessageType.CreateDialog, null);
        }

        private void AddFriend_Click(object sender, RoutedEventArgs e)
        {
            _client.SendMessage(MessageType.SendFriendRequest, FoundUsername.Text);
        }

        private void AcceptRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is FriendRequestInfo request)
            {
                _client.FriendRequestId = request.Id;
                _client.SendMessage(MessageType.AcceptFriendRequest, null);
            }
        }

        private void DeclineRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is FriendRequestInfo request)
            {
                _client.FriendRequestId = request.Id;
                _client.SendMessage(MessageType.DeclineFriendRequest, null);
            }
        }

    }
}
