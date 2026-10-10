using MessengerShared.Communication;

namespace MessengerShared.Models
{
    public class DialogInfo
    {
        public int Id { get; init; }
        public int? OtherUserId { get; init; }
        public string? Name { get; init; }
        public DialogType Type { get; init; }
    }
}
