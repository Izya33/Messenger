using MessengerShared.Communication;
using MessengerShared.Models;
using Microsoft.Data.SqlClient;

namespace MessengerDatabase.DataAccess
{
    public class DialogDataAccess
    {
        private readonly DatabaseConnection _database;

        public DialogDataAccess(DatabaseConnection database)
        {
            _database = database;
        }

        public int CreateDialog(String ?name, DialogType type)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("INSERT INTO Dialogs(Name, Type) OUTPUT INSERTED.Id VALUES(@name, @type)", connection);

            command.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            command.Parameters.AddWithValue("@type", type);

            int dialogId = (int)command.ExecuteScalar();

            return dialogId;
        }

        public void AddMember(int dialogId, int userId)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("INSERT INTO DialogMembers(DialogId, UserId) VALUES(@dialogId, @userId)", connection);

            command.Parameters.AddWithValue("@dialogId", dialogId);
            command.Parameters.AddWithValue("@userId", userId);

            command.ExecuteNonQuery();

        }

        public int? FindPrivateDialog(int userId1, int userId2)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT dm1.DialogId FROM DialogMembers dm1 JOIN DialogMembers dm2 ON dm1.DialogId = dm2.DialogId JOIN Dialogs d ON dm1.DialogId = d.Id WHERE dm1.UserId = @userId1 AND dm2.UserId = @userId2 AND d.Type = @type;", connection);

            command.Parameters.AddWithValue("@userId1", userId1);
            command.Parameters.AddWithValue("@userId2", userId2);
            command.Parameters.AddWithValue("@type", DialogType.Private);

            object? result = command.ExecuteScalar();

            if (result == null) return null;
            return (int)result;

        }

        public List<int> GetMembers(int dialogId)
        {
            List<int> result = new List<int>();

            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT UserId FROM DialogMembers WHERE DialogId = @dialogId", connection);

            command.Parameters.AddWithValue("@dialogId", dialogId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetInt32(0));
            }

            return result;
        }

        public List<DialogInfo> GetDialogs(int userId)
        {
            List<DialogInfo> result = new List<DialogInfo>();

            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT DISTINCT  d.Id, CASE  WHEN d.Type = 0 THEN u.Login  WHEN d.Type = 1 THEN d.Name END AS Name, d.Type,  CASE WHEN d.Type = 0 THEN otherDm.UserId ELSE NULL END AS OtherUserId FROM Dialogs d JOIN DialogMembers dm ON d.Id = dm.DialogId JOIN DialogMembers otherDm ON d.Id = otherDm.DialogId AND otherDm.UserId != @userId JOIN Users u  ON otherDm.UserId = u.Id WHERE dm.UserId = @userId", connection);

            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new DialogInfo
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Type = (DialogType)reader.GetInt32(2),
                    OtherUserId = reader.IsDBNull(3) ? null : reader.GetInt32(3)
                });
            }

            return result;
        }

    }
}
