namespace MessengerShared.Models
{
    public class StoredUser
    {
        public int Id { get; init; }

        public string Login { get; init; } = string.Empty;

        public string PasswordHash { get; init; } = string.Empty;

        public DateTime CreatedAt { get; init; }
    }
}
