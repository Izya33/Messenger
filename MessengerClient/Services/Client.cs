using MessengerShared.Communication;
using MessengerShared.Models;
using MessengerShared.Network;
using System.Net.Sockets;

namespace MessengerClient.Services
{
    public class Client
    {
        public String Sender { get; set; } = String.Empty;
        public String Receiver { get; set; } = String.Empty;
        public int DialogId { get; set; }
        public int FriendRequestId { get; set; }

        private MessageTransport _transport;

        private TcpClient? client;
        private NetworkStream stream = null;

        private Thread _clientThread;

        public event Action? LoginSucceeded;
        public event Action? RegisterSucceeded;
        public event Action<string>? ErrorReceived;
        public event Action<List<DialogInfo>>? UpdateDialog;
        public event Action<Message>? MessageReceived;
        public event Action<string, FriendState>? UserFound;
        public event Action<int>? DialogCreated;
        public event Action? FriendRequestSent;
        public event Action<List<FriendRequestInfo>>? UpdateRequestList;
        public event Action? RequestReceived;
        public event Action? RequestAccepted;
        public event Action? RequestDenied;
        public event Action<List<FriendsInfo>>? UpdateFriends;
        public event Action<int>? GroupCreated;

        public Client()
        {
            _transport = new MessageTransport();
            _clientThread = new Thread(new ThreadStart(ThreadFunc));
        }

        public void Connect(String server, Int32 port)
        {
            try
            {
                if (client == null)
                {
                    client = new TcpClient(server, port);
                    stream = client.GetStream();
                    _clientThread.Start();
                }
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException: {0}", ane);
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException: {0}", se);
            }
        }

        public void SendMessage(MessageType Type, String? text)
        {
            try
            {
                switch (Type)
                {
                    case MessageType.Register:
                        _transport.Send(stream, new Message { Type = MessageType.Register, Sender = this.Sender, Content = text });
                        break;

                    case MessageType.Login:
                        _transport.Send(stream, new Message { Type = MessageType.Login, Sender = this.Sender, Content = text });
                        break;

                    case MessageType.SendFriendRequest:
                        _transport.Send(stream, new Message { Type = MessageType.SendFriendRequest, Sender = this.Sender, Receiver = text});
                        break;

                    case MessageType.GetFriendRequests:
                        GetRequestProtocol(text);
                        break;

                    case MessageType.AcceptFriendRequest:
                        _transport.Send(stream, new Message { Type = MessageType.AcceptFriendRequest, Sender = this.Sender, FriendRequestId = FriendRequestId});
                        break;

                    case MessageType.DeclineFriendRequest:
                        _transport.Send(stream, new Message { Type = MessageType.DeclineFriendRequest, Sender = this.Sender, FriendRequestId = FriendRequestId });
                        break;

                    case MessageType.GetFriends:
                        _transport.Send(stream, new Message { Type = MessageType.GetFriends, Sender = this.Sender });
                        break;

                    case MessageType.GetDialogs:
                        _transport.Send(stream, new Message { Type = MessageType.GetDialogs, Sender = this.Sender});
                        break;

                    case MessageType.GetMessages:
                        _transport.Send(stream, new Message { Type = MessageType.GetMessages, Sender = this.Sender, DialogId = this.DialogId });
                        break;

                    case MessageType.CreateDialog:
                        _transport.Send(stream, new Message { Type = MessageType.CreateDialog, Receiver = this.Receiver });
                        break;

                    case MessageType.Message:
                        _transport.Send(stream, new Message { Type = MessageType.Message, DialogId = this.DialogId, Content = text });
                        break;

                    case MessageType.FindUser:
                        _transport.Send(stream, new Message { Type = MessageType.FindUser, Sender = this.Sender, Content = text });
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void CreateGroup(string groupName, List<int> memberIds)
        {
            _transport.Send(stream, new Message { Type = MessageType.CreateGroup, GroupName = groupName, GroupMemberId = memberIds});
        }

        public void ReceiveMessage()
        {
            try
            {
                Message? msg = _transport.Receive(stream);
                if (msg == null) return;

                switch (msg.Type)
                {
                    case MessageType.LoginSuccess:
                        LoginSucceeded?.Invoke();
                        break;

                    case MessageType.RegisterSuccess:
                        RegisterSucceeded?.Invoke();
                        break;

                    case MessageType.FriendRequestSent:
                        FriendRequestSent?.Invoke();
                        break;

                    case MessageType.UserFound:
                        if (msg.Receiver != null && msg.FriendState != null) 
                            UserFound?.Invoke(msg.Receiver, msg.FriendState.Value);
                        break;

                    case MessageType.DialogCreated:
                        if (msg.DialogId != null && msg.DialogId != DialogId) 
                            DialogCreatedProtocol(msg.DialogId.Value);
                        break;

                    case MessageType.Error:
                        if(msg.Content != null) 
                            ErrorReceived?.Invoke(msg.Content);
                        break;

                    case MessageType.Message:
                        MessageReceived?.Invoke(msg);
                        break;

                    case MessageType.GetDialogs:
                        if (msg.Dialogs != null)
                            UpdateDialog?.Invoke(msg.Dialogs);
                        break;

                    case MessageType.GetFriendRequests:
                        if (msg.FriendRequests != null) 
                            UpdateRequestList?.Invoke(msg.FriendRequests);
                        break;

                    case MessageType.FriendRequestReceived:
                        RequestReceived?.Invoke();
                        break;

                    case MessageType.RequestAccepted:
                        RequestAccepted?.Invoke();
                        break;

                    case MessageType.RequestDeclined:
                        RequestDenied?.Invoke();
                        break;

                    case MessageType.GetFriends:
                        if (msg.Friends != null) 
                            UpdateFriends?.Invoke(msg.Friends);
                        break;

                    case MessageType.GroupCreated:
                        if (msg.DialogId != null)
                            GroupCreated?.Invoke(msg.DialogId.Value);
                        break;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public Boolean Disconnect()
        {
            if (client != null)
            {
                stream.Close();
                client.Close();
                client = null;
                return true;
            }
            return false;
        }

        private void ThreadFunc()
        {
            while (true)
            {
                ReceiveMessage();
                if (client == null) return;
            }
        }

        private void GetRequestProtocol(string? text)
        {
            if (text == null) 
                _transport.Send(stream, new Message { Type = MessageType.GetFriendRequests, Sender = this.Sender });
            else 
                _transport.Send(stream, new Message { Type = MessageType.GetFriendRequests, Sender = this.Sender, Receiver = text });
        }

        private void DialogCreatedProtocol(int dialogId)
        {
            DialogId = dialogId;
            DialogCreated?.Invoke(DialogId);
        }

    }
}
