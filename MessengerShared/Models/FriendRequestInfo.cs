namespace MessengerShared.Models
{
    public class FriendRequestInfo
    {
        public int Id { get; init; }
        public int SenderId { get; init; }
        public string? SenderUsername { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
