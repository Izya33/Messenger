using MessengerServer.NetworkItems;

namespace MessengerServer.Handlers
{
    internal class ConnectedUserManager
    {
        private List<ConnectedUser> _connectedUsers;

        public ConnectedUserManager()
        {
            _connectedUsers = new List<ConnectedUser>();
        }

        public void Add(ConnectedUser user)
        {
            _connectedUsers.Add(user);
        }

        public bool Remove(ConnectedUser user) 
        {
            return _connectedUsers.Remove(user);
        }

        public ConnectedUser? GetUserById(int id)
        {
            return _connectedUsers.Find(x => x.Id == id);
        }

        public ConnectedUser? GetUserByLogin(string username)
        {
            return _connectedUsers.Find(x => x.Username == username);
        }

        public bool Exists(string username)
        {
            if (_connectedUsers.Find(x => x.Username == username) != null)
                return true;
            return false;
        }
    }
}
