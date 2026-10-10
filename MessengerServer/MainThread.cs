using System.Net;
using MessengerDatabase;
using MessengerServer.MainServer;

namespace MessengerServer
{
    class MainThread
    {
        public static void Main()
        {
            var database = new DatabaseConnection();
            database.Initialize();

            Server server = new Server(database);
            server.Start(IPAddress.Parse("127.0.0.1"), 13000);
        }
    }
}
