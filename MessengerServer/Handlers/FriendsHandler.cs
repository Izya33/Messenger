using MessengerDatabase.DataAccess;
using MessengerServer.NetworkItems;
using MessengerShared.Communication;
using MessengerShared.Network;

namespace MessengerServer.Handlers
{
    internal class FriendsHandler
    {
        private readonly FriendRequestDataAccess _friendRequestDataAccess;
        private readonly UserDataAccess _userDataAccess;
        private readonly MessageTransport _transport;
        private readonly ConnectedUserManager _connectedUserManager;

        public FriendsHandler(FriendRequestDataAccess friendRequestRepository, UserDataAccess userRepository, MessageTransport transport, ConnectedUserManager connectedUserManager)
        {
            _friendRequestDataAccess = friendRequestRepository;
            _userDataAccess = userRepository;
            _transport = transport;
            _connectedUserManager = connectedUserManager;
        }

        //TODO: Else Server answer
        public void SendRequest(Message msg, ConnectedUser currentUser)
        {
            if (msg.Receiver == null) return;

            var receiver = _userDataAccess.GetByLogin(msg.Receiver);
            if (receiver == null) return;

            if (_friendRequestDataAccess.CreateRequest(currentUser.Id, receiver.Id))
            {
                _transport.Send(currentUser.Connection, new Message { Type = MessageType.FriendRequestSent, Sender = currentUser.Username, Receiver = receiver.Login });

                ConnectedUser? receiverUser = _connectedUserManager.GetUserByLogin(receiver.Login);
                if(receiverUser != null)
                    _transport.Send(receiverUser.Connection, new Message { Type = MessageType.FriendRequestReceived, Sender = currentUser.Username, Receiver = receiverUser.Username });
            }
        }

        public void GetRequests(ConnectedUser currentUser)
        {
            var list = _friendRequestDataAccess.GetPendingRequests(currentUser.Id);

            _transport.Send(currentUser.Connection, new Message { Type = MessageType.GetFriendRequests, Receiver = currentUser.Username, FriendRequests = list});
        }

        public void GetFriends(ConnectedUser currentUser)
        {
            var list = _friendRequestDataAccess.GetFriends(currentUser.Id);

            _transport.Send(currentUser.Connection, new Message { Type = MessageType.GetFriends, Receiver = currentUser.Username, Friends = list});
        }

        //TODO: Else Server answer
        public void AcceptRequest(Message msg, ConnectedUser currentUser)
        {
            if (msg.FriendRequestId == null ) return;

            var request = _friendRequestDataAccess.GetRequestById(msg.FriendRequestId.Value);
            if(request == null ) return;

            var senderId = request.SenderId;

            if (_friendRequestDataAccess.AcceptRequest(msg.FriendRequestId.Value, currentUser.Id))
            {
                ConnectedUser? requestSender = _connectedUserManager.GetUserById(senderId);
                if (requestSender != null)
                    _transport.Send(requestSender.Connection, new Message { Type = MessageType.RequestAccepted, Sender = currentUser.Username, Receiver = requestSender.Username });
            }
                
        }

        //TODO: Else Server answer
        public void DeclineRequest(Message msg, ConnectedUser currentUser)
        {
            if (msg.FriendRequestId == null) return;

            var request = _friendRequestDataAccess.GetRequestById(msg.FriendRequestId.Value);
            if (request == null) return;

            var senderId = request.SenderId;

            if (_friendRequestDataAccess.DeclineRequest(msg.FriendRequestId.Value, currentUser.Id))
            {
                ConnectedUser? requestSender = _connectedUserManager.GetUserById(senderId);
                if (requestSender != null)
                    _transport.Send(requestSender.Connection, new Message { Type = MessageType.RequestDeclined, Sender = currentUser.Username, Receiver = requestSender.Username });
            }
        }

        public void FindUser(Message msg, ConnectedUser currentUser)
        {
            if (msg.Content == null) return;

            var user = _userDataAccess.GetByLogin(msg.Content);

            if (user == null)
            {
                _transport.Send(currentUser.Connection, new Message { Type = MessageType.Error, Content = "User not found" });
                return;
            }

            FriendState state = FriendState.None;

            if (_friendRequestDataAccess.AreFriends(currentUser.Id, user.Id))
                state = FriendState.Friend;
            else if (_friendRequestDataAccess.HasPendingRequest(currentUser.Id, user.Id))
                state = FriendState.Pending;

            _transport.Send(currentUser.Connection, new Message { Type = MessageType.UserFound, Receiver = user.Login , FriendState = state });
        }
    }
}
