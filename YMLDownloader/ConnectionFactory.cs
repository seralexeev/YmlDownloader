using System.Data;
using System.Data.SqlClient;

namespace YMLDownloader
{
    public class ConnectionFactory
    {
        private string _connectionString;

        public ConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection Create() =>
            new SqlConnection(_connectionString);
    }
}
