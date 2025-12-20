namespace Inventory.API.Services
{
    public interface IDatabaseFactoryService
    {
        Task<IDbConnection> CreateConnectionAsync();
        IDbConnection CreateConnection();
    }
    public class DatabaseFactoryService : IDatabaseFactoryService
    {
        private readonly string _connectionString;
        public DatabaseFactoryService(string connectionString) => _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        public IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
