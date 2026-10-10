using System.Net.Sockets;

namespace MessengerServer.NetworkItems
{
    public class ConnectedUser
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public NetworkStream Connection { get; private set; }

        public ConnectedUser(int id, string name, NetworkStream connection)
        {
            Id = id;
            Username = name;
            Connection = connection;
        }

    }
}
