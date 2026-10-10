using MessengerShared.Models;
using Microsoft.Data.SqlClient;

namespace MessengerDatabase.DataAccess
{
    public class MessageDataAccess
    {
        private readonly DatabaseConnection _database;

        public MessageDataAccess(DatabaseConnection database)
        {
            _database = database;
        }

        public void SaveMessage(int dialogId, int senderId, string content)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("INSERT INTO Messages(DialogId, SenderId, Content) VALUES (@dialogId, @senderId, @content)", connection);

            command.Parameters.AddWithValue("@dialogId", dialogId);
            command.Parameters.AddWithValue("@senderId", senderId);
            command.Parameters.AddWithValue("@content", content);

            command.ExecuteNonQuery();
        }

        public List<StoredMessage> GetMessages(int dialogId)
        {
            List<StoredMessage> result = new List<StoredMessage>();

            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT m.Id, m.DialogId, m.SenderId, m.Content, m.SentAt, m.IsEdited, u.Login FROM Messages m JOIN Users u ON m.SenderId = u.Id WHERE DialogId = @dialogId ORDER BY m.SentAt ASC", connection);

            command.Parameters.AddWithValue("@dialogId", dialogId);

            using var reader = command.ExecuteReader();

            while(reader.Read())
            {
                result.Add(new StoredMessage { 
                    Id = reader.GetInt32(0), 
                    DialogId = reader.GetInt32(1), 
                    SenderId = reader.GetInt32(2), 
                    Content = reader.GetString(3), 
                    SentAt = reader.GetDateTime(4), 
                    IsEdited = reader.GetBoolean(5),
                    SenderUsername = reader.GetString(6)
                });
            }

            return result;
        }

    }
}
