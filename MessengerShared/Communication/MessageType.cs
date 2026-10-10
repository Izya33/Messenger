namespace MessengerShared.Communication
{
    public enum MessageType
    {
        Register,
        RegisterSuccess,
        Login,
        LoginSuccess,

        SendFriendRequest,
        FriendRequestSent,
        GetFriendRequests,
        FriendRequestReceived,
        AcceptFriendRequest,
        RequestAccepted,
        DeclineFriendRequest,
        RequestDeclined,
        GetFriends,

        GetDialogs,
        GetMessages,

        FindUser,
        UserFound,

        CreateDialog,
        DialogCreated,

        CreateGroup,
        GroupCreated,


        Message,
        Error,

        Disconnect
    }
}
