using Microsoft.Data.SqlClient;

namespace MessengerDatabase
{
    public class DatabaseConnection
    {
        private readonly string _connection;
        public DatabaseConnection()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = @"localhost\SQLEXPRESS",
                InitialCatalog = "MessengerDB",
                IntegratedSecurity = true,
                Encrypt = true,
                TrustServerCertificate = true
            };

            _connection = builder.ConnectionString;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connection);
        }

        public void Initialize()
        {
            using var connection = CreateConnection();
            connection.Open();

            string sql = File.ReadAllText("Database/InitiateDatabase.sql");

            using var command = new SqlCommand(sql, connection);

            command.ExecuteNonQuery();
        }
    }
}
