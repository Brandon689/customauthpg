using customauthpg.Models;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace customauthpg.Repositories;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IOptions<ConnectionStrings> connectionStrings)
    {
        _connectionString = connectionStrings.Value.DefaultConnection;
    }

    public async Task<User> GetUserByUsername(string username)
    {
        const string sql = "SELECT * FROM users WHERE username = @username";
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("username", username);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUserFromReader(reader);
        }
        return null;
    }

    public async Task<User> GetUserById(int userId)
    {
        const string sql = "SELECT * FROM users WHERE id = @userId";
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUserFromReader(reader);
        }
        return null;
    }

    public async Task<bool> CreateUser(User user)
    {
        const string sql = @"
            INSERT INTO users (username, email, password_hash, role)
            VALUES (@username, @email, @passwordHash, @role)";

        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("username", user.Username);
        cmd.Parameters.AddWithValue("email", user.Email);
        cmd.Parameters.AddWithValue("passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("role", user.Role);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private User MapUserFromReader(IDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Username = reader.GetString(reader.GetOrdinal("username")),
            Email = reader.GetString(reader.GetOrdinal("email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
            Role = reader.GetString(reader.GetOrdinal("role")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }
}