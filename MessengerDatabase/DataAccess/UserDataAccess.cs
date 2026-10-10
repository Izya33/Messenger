using Microsoft.Data.SqlClient;

using MessengerShared.Models;

namespace MessengerDatabase.DataAccess
{
    public class UserDataAccess
    {
        private readonly DatabaseConnection _database;

        public UserDataAccess(DatabaseConnection database)
        {
            _database = database;
        }

        public void CreateUser(StoredUser user)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("INSERT INTO Users(Login, PasswordHash) VALUES(@login,@passwordHash)", connection);

            command.Parameters.AddWithValue("@login", user.Login);
            command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);

            command.ExecuteNonQuery();
        }

        public bool Exists(string login)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Login = @login", connection);

            command.Parameters.AddWithValue("@login", login);

            int count = (int)command.ExecuteScalar();

            return count > 0;
        }

        public StoredUser? GetByLogin(string login)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT Id, Login, PasswordHash, CreatedAt FROM Users WHERE Login = @login", connection);

            command.Parameters.AddWithValue("@login", login);

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return new StoredUser
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3)
            };
        }

        public StoredUser? GetById(int id)
        {
            using var connection = _database.CreateConnection();
            connection.Open();

            using var command = new SqlCommand("SELECT Id, Login, PasswordHash, CreatedAt FROM Users WHERE Id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return new StoredUser
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3)
            };
        }

    }
}
