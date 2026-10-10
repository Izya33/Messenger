using MessengerDatabase.DataAccess;
using MessengerShared.Communication;
using MessengerShared.Models;
using MessengerShared.Network;
using System.Net.Sockets;

namespace MessengerServer.Handlers
{
    internal class RegisterHandler
    {
        private readonly UserDataAccess _userDataAccess;
        private readonly MessageTransport _transport;

        public RegisterHandler(UserDataAccess userRepository, MessageTransport transport)
        {
            _userDataAccess = userRepository;
            _transport = transport;
        }

        public void Handle(Message msg, NetworkStream stream) 
        {
            if (msg.Sender == null) return;

            if (_userDataAccess.Exists(msg.Sender))
            {
                _transport.Send(stream, new Message { Type = MessageType.Error, Receiver = msg.Sender, Content = "User already exist! " });
            }
            else
            {
                if (msg.Content == null) return;
                _userDataAccess.CreateUser(new StoredUser { Login = msg.Sender, PasswordHash = msg.Content });
                _transport.Send(stream, new Message { Type = MessageType.RegisterSuccess, Receiver = msg.Sender, Content = "User succsesfully registered! " });
            }
        }

    }
}
