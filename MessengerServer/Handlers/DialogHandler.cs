using MessengerDatabase.DataAccess;
using MessengerServer.NetworkItems;
using MessengerShared.Communication;
using MessengerShared.Models;
using MessengerShared.Network;

namespace MessengerServer.Handlers
{
    internal class DialogHandler
    {
        private readonly UserDataAccess _userDataAccess;
        private readonly DialogDataAccess _dialogDataAccess;
        private readonly MessageTransport _transport;
        private readonly ConnectedUserManager _connectedUserManager;

        public DialogHandler(UserDataAccess userRepository, DialogDataAccess dialogRepository, MessageTransport transport, ConnectedUserManager connectedUserManager)
        {
            _userDataAccess = userRepository;
            _dialogDataAccess = dialogRepository;
            _transport = transport;
            _connectedUserManager = connectedUserManager;
        }

        public void HandlePrivateDialog(Message msg, ConnectedUser currentUser)
        {
            if(msg.Receiver == null ) return;

            StoredUser? receiverUser = _userDataAccess.GetByLogin(msg.Receiver);

            if (receiverUser == null)
            {
                _transport.Send(currentUser.Connection, new Message { Type = MessageType.Error, Receiver = currentUser.Username, Content = "User cannot be find! " });
                return;
            }

            int? dialogId = _dialogDataAccess.FindPrivateDialog(currentUser.Id, receiverUser.Id);

            if (dialogId == null)
            {
                dialogId = _dialogDataAccess.CreateDialog(null, DialogType.Private);
                _dialogDataAccess.AddMember((int)dialogId, currentUser.Id);
                _dialogDataAccess.AddMember((int)dialogId, receiverUser.Id);
            }

            _transport.Send(currentUser.Connection, new Message { Type = MessageType.DialogCreated, Receiver = currentUser.Username, DialogId = dialogId });

        }

        public void CreateGroupHandler(Message msg, ConnectedUser currentUser)
        {
            if (msg.GroupMemberId == null || msg.GroupName == null ) return;

            HashSet<int> memberIds = new HashSet<int>();

            memberIds.Add(currentUser.Id);
            foreach (int memberId in msg.GroupMemberId)
                memberIds.Add(memberId);


            List<StoredUser> members = new List<StoredUser>();

            foreach (int memberId in memberIds)
            {
                StoredUser? member = _userDataAccess.GetById(memberId);

                if (member != null)
                    members.Add(member);
            }

            int? dialogId = _dialogDataAccess.CreateDialog(msg.GroupName, DialogType.Group);
            if (dialogId == null) return;

            foreach (StoredUser member in members)
            {
                _dialogDataAccess.AddMember(dialogId.Value, member.Id);

                ConnectedUser? memberOnline = _connectedUserManager.GetUserById(member.Id);
                if (memberOnline != null)
                    _transport.Send(memberOnline.Connection, new Message { Type = MessageType.GroupCreated, DialogId = dialogId, Receiver = memberOnline.Username });
            }
              
        }

        public void GetDialogs(ConnectedUser currentUser)
        {
            List<DialogInfo> list = _dialogDataAccess.GetDialogs(currentUser.Id);
            _transport.Send(currentUser.Connection, new Message { Type = MessageType.GetDialogs, Sender = currentUser.Username, Dialogs = list });
        }
    }
}
