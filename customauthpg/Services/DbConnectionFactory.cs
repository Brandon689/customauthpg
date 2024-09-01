using Npgsql;
using Microsoft.Extensions.Configuration;

namespace YourNamespace.Services
{
    public class DbConnectionFactory
    {
        private static readonly NpgsqlDataSource _dataSource;

        static DbConnectionFactory()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DefaultConnection"));

            // Configure connection pooling (optional, as it's enabled by default)
            //dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 100;
            //dataSourceBuilder.ConnectionStringBuilder.MinPoolSize = 1;

            _dataSource = dataSourceBuilder.Build();
        }

        public static NpgsqlConnection CreateConnection() => _dataSource.CreateConnection();
    }
}
