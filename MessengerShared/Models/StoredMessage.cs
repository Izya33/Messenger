namespace MessengerShared.Models
{
    public class StoredMessage
    {
        public int Id { get; init; }
        public int DialogId { get; init; }
        public int SenderId { get; init; }
        public string SenderUsername { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public DateTime SentAt { get; init; }
        public bool IsEdited { get; init; }

    }
}
