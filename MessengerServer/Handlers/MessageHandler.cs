using MessengerDatabase.DataAccess;
using MessengerServer.NetworkItems;
using MessengerShared.Communication;
using MessengerShared.Models;
using MessengerShared.Network;

namespace MessengerServer.Handlers
{
    internal class MessageHandler
    {
        private readonly DialogDataAccess _dialogDataAccess;
        private readonly MessageDataAccess _messageDataAccess;
        private readonly ConnectedUserManager _connectedUserManager;
        private readonly MessageTransport _transport;
        public MessageHandler(DialogDataAccess dialogRepository, MessageDataAccess messageRepository, ConnectedUserManager connectedUserManager, MessageTransport transport) 
        {
            _dialogDataAccess = dialogRepository;
            _messageDataAccess = messageRepository;
            _connectedUserManager = connectedUserManager;
            _transport = transport;
        }

        //Stream array: 0 for Message Sender and 1 for Message Receiver.
        //TODO: User sended wrong DialogId
        public void Handle(Message msg, ConnectedUser currentUser)
        {
            if (msg.Content == null) return;

            int? dialogId = msg.DialogId;
            if (dialogId == null) return;

            List<int> membersId = _dialogDataAccess.GetMembers(dialogId.Value);

            if(membersId.Contains(currentUser.Id))
            {
                _messageDataAccess.SaveMessage(dialogId.Value, currentUser.Id, msg.Content);
                foreach(var id in membersId)
                {
                    ConnectedUser? onlineUser = _connectedUserManager.GetUserById(id);
                    if(onlineUser != null)
                        _transport.Send(onlineUser.Connection, new Message { Type = MessageType.Message, Sender = currentUser.Username, Content = msg.Content, DialogId = dialogId });
                }
            } 
        }

        public void GetMesseges(Message msg, ConnectedUser currentUser)
        {
            if (msg.DialogId == null) return;

            List<int> membersId = _dialogDataAccess.GetMembers(msg.DialogId.Value);

            if (membersId.Contains(currentUser.Id))
            {
                List<StoredMessage> messages = _messageDataAccess.GetMessages(msg.DialogId.Value);

                foreach (var message in messages)
                {
                    _transport.Send(currentUser.Connection, new Message { Type = MessageType.Message, Sender = message.SenderUsername, Content = message.Content, DialogId = msg.DialogId.Value });
                }
            }

        }

    }

}
