using MessengerShared.Communication;
using MessengerShared.Models;
using Microsoft.Data.SqlClient;

namespace MessengerDatabase.DataAccess
{
    public class FriendRequestDataAccess
    {
        private readonly DatabaseConnection _database;

        public FriendRequestDataAccess(DatabaseConnection database)
        {
            _database = database;
        }

        public bool CreateRequest(int senderId, int receiverId)
        {
            if (senderId == receiverId)
                return false;

            if (AreFriends(senderId, receiverId))
                return false;

            if (HasPendingRequest(senderId, receiverId))
                return false;

            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("INSERT INTO FriendRequests(SenderId, ReceiverId, Status) VALUES (@senderId, @receiverId, @status)", connection);

            command.Parameters.AddWithValue("@senderId", senderId);
            command.Parameters.AddWithValue("@receiverId", receiverId);
            command.Parameters.AddWithValue("@status", FriendRequestStatus.Pending);

            command.ExecuteNonQuery();
            return true;
        }

        public bool AcceptRequest(int requestId, int userId)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("UPDATE FriendRequests SET Status = @accepted WHERE Id = @requestId AND ReceiverId = @userId AND Status = @status", connection);

            command.Parameters.AddWithValue("@requestId", requestId);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@accepted", FriendRequestStatus.Accepted);
            command.Parameters.AddWithValue("@status", FriendRequestStatus.Pending);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows > 0;
        }

        public bool DeclineRequest(int requestId, int userId)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("UPDATE FriendRequests SET Status = @declined WHERE Id = @requestId AND ReceiverId = @userId AND Status = @status", connection);

            command.Parameters.AddWithValue("@requestId", requestId);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@declined", FriendRequestStatus.Declined);
            command.Parameters.AddWithValue("@status", FriendRequestStatus.Pending);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows > 0;
        }

        public bool AreFriends(int userId1, int userId2)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT COUNT(*) FROM FriendRequests WHERE ((SenderId = @userId1 AND ReceiverId = @userId2) OR (SenderId = @userId2 AND ReceiverId = @userId1)) AND Status = @status", connection);

            command.Parameters.AddWithValue("@userId1", userId1);
            command.Parameters.AddWithValue("@userId2", userId2);
            command.Parameters.AddWithValue("@status", FriendRequestStatus.Accepted);

            int count = (int)command.ExecuteScalar();

            return count > 0;
        }

        public bool HasPendingRequest(int senderId, int receiverId)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT COUNT(*) FROM FriendRequests WHERE ( (SenderId = @senderId AND ReceiverId = @receiverId) OR (SenderId = @receiverId AND ReceiverId = @senderId) )  AND Status = @status", connection);

            command.Parameters.AddWithValue("@senderId", senderId);
            command.Parameters.AddWithValue("@receiverId", receiverId);
            command.Parameters.AddWithValue("@status", FriendRequestStatus.Pending);

            int count = (int)command.ExecuteScalar();

            return count > 0;
        }

        public List<FriendRequestInfo> GetPendingRequests(int userId)
        {
            List<FriendRequestInfo> list = new();

            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT fr.Id, fr.SenderId, u.Login, fr.CreatedAt FROM FriendRequests fr JOIN Users u ON fr.SenderId = u.Id WHERE fr.ReceiverId = @userId AND fr.Status = @status", connection);

            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@status", FriendRequestStatus.Pending);

            using var reader = command.ExecuteReader();

            while (reader.Read())
                list.Add(new FriendRequestInfo { Id = reader.GetInt32(0), SenderId = reader.GetInt32(1), SenderUsername = reader.GetString(2), CreatedAt = reader.GetDateTime(3) });

            return list;

        }

        public List<FriendsInfo> GetFriends(int userId)
        {
            List<FriendsInfo> list = new();

            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT CASE WHEN fr.SenderId = @userId THEN fr.ReceiverId ELSE fr.SenderId END AS FriendId, u.Login FROM FriendRequests fr JOIN Users u ON u.Id = CASE  WHEN fr.SenderId = @userId THEN fr.ReceiverId ELSE fr.SenderId END WHERE (fr.SenderId = @userId OR fr.ReceiverId = @userId) AND fr.Status = @status", connection);

            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@status", FriendRequestStatus.Accepted);

            using var reader = command.ExecuteReader();

            while (reader.Read())
                list.Add(new FriendsInfo { Id = reader.GetInt32(0), Username = reader.GetString(1)});

            return list;
        }

        public StoredFriendRequest? GetRequestById(int requestId)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT Id, SenderId, ReceiverId, Status FROM FriendRequests WHERE Id = @requestId", connection);

            command.Parameters.AddWithValue("@requestId", requestId);

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return new StoredFriendRequest 
            {
                Id = reader.GetInt32(0),
                SenderId = reader.GetInt32(1),
                ReceiverId = reader.GetInt32(2),
                Status = (FriendRequestStatus)reader.GetInt32(3)
            };

        }


    }
}
