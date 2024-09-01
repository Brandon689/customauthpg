using customauthpg.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace customauthpg.Services;

public class DatabaseInitializer
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly string _connectionString;

    public DatabaseInitializer(IOptions<ConnectionStrings> connectionStrings, ILogger<DatabaseInitializer> logger)
    {
        _connectionString = connectionStrings.Value.DefaultConnection;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            _logger.LogError("Connection string is not properly configured.");
            throw new InvalidOperationException("Connection string is not properly configured.");
        }

        await CreateTablesAsync();
        await SeedDataAsync();
    }

    private async Task CreateTablesAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
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

    private async Task SeedDataAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Check if we need to seed data
        var checkCommand = new NpgsqlCommand("SELECT COUNT(*) FROM users", connection);
        var userCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

        //if (userCount == 0)
        {
            // Seed users
            var users = new List<User>
            {
                new User { Username = "admin", Email = "admin@example.com", PasswordHash = BC.HashPassword("adminpassword"), Role = "Admin" },
                new User { Username = "user1", Email = "user1@example.com", PasswordHash = BC.HashPassword("password1"), Role = "User" },
                new User { Username = "user2", Email = "user2@example.com", PasswordHash = BC.HashPassword("password2"), Role = "User" },
                new User { Username = "user3", Email = "user3@example.com", PasswordHash = BC.HashPassword("password3"), Role = "User" }
            };

            foreach (var user in users)
            {
                var seedCommand = new NpgsqlCommand(@"
                        INSERT INTO users (username, email, password_hash, role)
                        VALUES (@username, @email, @passwordHash, @role)", connection);

                seedCommand.Parameters.AddWithValue("username", user.Username);
                seedCommand.Parameters.AddWithValue("email", user.Email);
                seedCommand.Parameters.AddWithValue("passwordHash", user.PasswordHash);
                seedCommand.Parameters.AddWithValue("role", user.Role);

                await seedCommand.ExecuteNonQueryAsync();
                _logger.LogInformation($"{user.Role} user '{user.Username}' seeded.");
            }

            _logger.LogInformation("All seed users have been added.");
        }
    }
}
