using MessengerShared.Communication;

namespace MessengerShared.Models
{
    public class StoredFriendRequest
    {
        public int Id { get; init; }
        public int SenderId { get; init; }
        public int ReceiverId { get; init; }
        public FriendRequestStatus Status { get; init; }
    }
}
