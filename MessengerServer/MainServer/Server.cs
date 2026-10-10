using MessengerDatabase;
using MessengerDatabase.DataAccess;
using MessengerServer.Handlers;
using MessengerServer.NetworkItems;
using MessengerShared.Communication;
using MessengerShared.Network;
using System.Net;
using System.Net.Sockets;

namespace MessengerServer.MainServer
{
    class Server
    {
        private readonly MessageTransport _transport;

        private readonly UserDataAccess _userDataAccess;
        private readonly DialogDataAccess _dialogDataAccess;
        private readonly MessageDataAccess _messageDataAccess;
        private readonly FriendRequestDataAccess _friendRequestDataAccess;

        private readonly ConnectedUserManager _userManager;
        private readonly RegisterHandler _registerHandler;
        private readonly LoginHandler _loginHandler;
        private readonly MessageHandler _messageHandler;
        private readonly DialogHandler _dialogHandler;
        private readonly FriendsHandler _friendsHandler;

        public Server(DatabaseConnection database)
        {
            _transport = new MessageTransport();

            _userDataAccess = new UserDataAccess(database);
            _dialogDataAccess = new DialogDataAccess(database);
            _messageDataAccess = new MessageDataAccess(database);
            _friendRequestDataAccess = new FriendRequestDataAccess(database);

            _userManager = new ConnectedUserManager();
            _registerHandler = new RegisterHandler(_userDataAccess, _transport);
            _loginHandler = new LoginHandler(_userDataAccess, _transport);
            _dialogHandler = new DialogHandler(_userDataAccess, _dialogDataAccess, _transport, _userManager);
            _messageHandler = new MessageHandler(_dialogDataAccess, _messageDataAccess, _userManager, _transport);
            _friendsHandler = new FriendsHandler(_friendRequestDataAccess, _userDataAccess, _transport, _userManager);
        }

        public void Start(IPAddress adress, Int32 port)
        {
            TcpListener? listener = null;

            try
            {
                listener = new TcpListener(adress, port);
                listener.Start();

                while (true)
                {
                    Console.Write("Waiting for conection... ");

                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    Thread thread = new Thread(() => HandleClient(client));
                    thread.Start();

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (listener != null) listener.Stop();
            }
            Console.Read();
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream senderStream = client.GetStream();

            if (senderStream.CanRead)
            {
                ConnectedUser? currentUser = null;

                try
                {
                    while (true)
                    {
                        Message? msg = _transport.Receive(senderStream);
                        if (msg == null) return;

                        switch (msg.Type)
                        {
                            case MessageType.Register:
                                _registerHandler.Handle(msg, senderStream);
                                break;

                            case MessageType.Login:
                                if ((currentUser = _loginHandler.Handle(msg, senderStream)) != null)
                                    LoginProtocol(currentUser);
                                break;

                            case MessageType.FindUser:
                                if (currentUser != null)
                                    _friendsHandler.FindUser(msg, currentUser);
                                break;

                            case MessageType.GetFriends:
                                if(currentUser != null)
                                    _friendsHandler.GetFriends(currentUser);
                                break;

                            case MessageType.GetFriendRequests:
                                if(currentUser != null)
                                    _friendsHandler.GetRequests(currentUser);
                                break;

                            case MessageType.SendFriendRequest:
                                if (currentUser != null)
                                    _friendsHandler.SendRequest(msg, currentUser);
                                break;

                            case MessageType.AcceptFriendRequest:
                                if (currentUser != null)
                                    AcceptRequestProtocol(msg, currentUser);
                                break;

                            case MessageType.DeclineFriendRequest:
                                if (currentUser != null)
                                    DeclineRequestProtocol(msg, currentUser);
                                break;

                            case MessageType.GetDialogs:
                                if (currentUser != null)
                                    _dialogHandler.GetDialogs(currentUser);
                                break;

                            case MessageType.GetMessages:
                                if (currentUser != null)
                                    _messageHandler.GetMesseges(msg, currentUser);
                                break;

                            case MessageType.CreateDialog:
                                if(currentUser != null)
                                    _dialogHandler.HandlePrivateDialog(msg, currentUser);
                                break;

                            case MessageType.CreateGroup:
                                if (currentUser != null)
                                    _dialogHandler.CreateGroupHandler(msg, currentUser);
                                break;

                            case MessageType.Message:
                                if (currentUser != null)
                                    _messageHandler.Handle(msg, currentUser);
                                break;

                            case MessageType.Disconnect:
                                return;

                        }
                    }

                }catch(Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                }
                finally
                {
                    if(currentUser != null) _userManager.Remove(currentUser);
                }

            }

        }

        private void LoginProtocol(ConnectedUser currentUser)
        {
            _userManager.Add(currentUser);
            _dialogHandler.GetDialogs(currentUser);
            _friendsHandler.GetRequests(currentUser);
            _friendsHandler.GetFriends(currentUser);
        }

        private void AcceptRequestProtocol(Message msg, ConnectedUser currentUser)
        {
            _friendsHandler.AcceptRequest(msg, currentUser);
            _friendsHandler.GetRequests(currentUser);
            _friendsHandler.GetFriends(currentUser);
        }

        private void DeclineRequestProtocol(Message msg, ConnectedUser currentUser)
        {
            _friendsHandler.DeclineRequest(msg, currentUser);
            _friendsHandler.GetRequests(currentUser);
            _friendsHandler.GetFriends(currentUser);
        }

    }
}