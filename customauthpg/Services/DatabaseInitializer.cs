using Npgsql;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace YourNamespace.Services
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            var adminConnectionString = _configuration.GetConnectionString("AdminConnection");
            var appConnectionString = _configuration.GetConnectionString("DefaultConnection");

            await CreateDatabaseAndUserAsync(adminConnectionString);
            await CreateTablesAsync(appConnectionString);
        }

        private async Task CreateDatabaseAndUserAsync(string adminConnectionString)
        {
            var dbName = _configuration["Database:Name"];
            var dbUser = _configuration["Database:User"];
            var dbPassword = _configuration["Database:Password"];

            using var connection = new NpgsqlConnection(adminConnectionString);
            await connection.OpenAsync();

            // Create database
            try
            {
                using var createDbCommand = new NpgsqlCommand($"CREATE DATABASE {dbName}", connection);
                await createDbCommand.ExecuteNonQueryAsync();
                _logger.LogInformation($"Database {dbName} created successfully.");
            }
            catch (PostgresException ex) when (ex.SqlState == "42P04") // 42P04 = duplicate_database
            {
                _logger.LogInformation($"Database {dbName} already exists.");
            }

            // Create user
            try
            {
                using var createUserCommand = new NpgsqlCommand($"CREATE USER {dbUser} WITH ENCRYPTED PASSWORD '{dbPassword}'", connection);
                await createUserCommand.ExecuteNonQueryAsync();
                _logger.LogInformation($"User {dbUser} created successfully.");
            }
            catch (PostgresException ex) when (ex.SqlState == "42710") // 42710 = duplicate_object
            {
                _logger.LogInformation($"User {dbUser} already exists.");
            }

            // Grant privileges
            using var grantCommand = new NpgsqlCommand($"GRANT ALL PRIVILEGES ON DATABASE {dbName} TO {dbUser}", connection);
            await grantCommand.ExecuteNonQueryAsync();
            _logger.LogInformation($"Privileges granted to {dbUser} on {dbName}.");
        }

        private async Task CreateTablesAsync(string appConnectionString)
        {
            using var connection = new NpgsqlConnection(appConnectionString);
            await connection.OpenAsync();

            await CreateUsersTableAsync(connection);
            // Add more table creation methods as needed
        }

        private async Task CreateUsersTableAsync(NpgsqlConnection connection)
        {
            var command = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    username VARCHAR(50) UNIQUE NOT NULL,
                    email VARCHAR(100) UNIQUE NOT NULL,
                    password_hash VARCHAR(255) NOT NULL,
                    role VARCHAR(20) NOT NULL,
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                )", connection);

            await command.ExecuteNonQueryAsync();
            _logger.LogInformation("Users table created or already exists.");
        }

        // You can add more methods to create additional tables or perform other initialization tasks
    }
}
