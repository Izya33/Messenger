using MessengerDatabase.DataAccess;
using MessengerServer.NetworkItems;
using MessengerShared.Communication;
using MessengerShared.Models;
using MessengerShared.Network;
using System.Net.Sockets;

namespace MessengerServer.Handlers
{
    class LoginHandler
    {
        private readonly UserDataAccess _userDataAccess;
        private readonly MessageTransport _transport;
        public LoginHandler(UserDataAccess userRepository, MessageTransport transport) 
        {
            _userDataAccess = userRepository;
            _transport = transport;
        }

        public ConnectedUser? Handle(Message msg, NetworkStream stream)
        {
            if (msg.Sender == null) return null;
            StoredUser? user = _userDataAccess.GetByLogin(msg.Sender);

            if (user == null)
            {
                _transport.Send(stream, new Message { Type = MessageType.Error, Receiver = msg.Sender, Content = "No user with this Login exist! " });
                return null;
            }

            //TODO: verify password hash
            if (!user.PasswordHash.Equals(msg.Content))
            {
                _transport.Send(stream, new Message { Type = MessageType.Error, Receiver = msg.Sender, Content = "Wrong password! " });
                return null;
            }

            _transport.Send(stream, new Message { Type = MessageType.LoginSuccess, Receiver = msg.Sender, Content = "User logged in! " });
            return new ConnectedUser(user.Id, user.Login, stream);
        }
    }
}
