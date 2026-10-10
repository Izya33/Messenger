using MessengerShared.Models;

namespace MessengerShared.Communication
{
    public class Message
    {
        public MessageType Type {  get; set; }


        public String? Sender { get; set; }
        public String? Receiver { get; set; }
        public String? Content { get; set; }


        public int? DialogId { get; set; }
        public List<DialogInfo>? Dialogs { get; set; }


        public int? FriendRequestId { get; set; }
        public List<FriendRequestInfo>? FriendRequests { get; set; }


        public FriendState? FriendState { get; set; }
        public List<FriendsInfo>? Friends { get; set; }


        public string? GroupName { get; set; }
        public List<int>? GroupMemberId { get; set; }

    }
}
