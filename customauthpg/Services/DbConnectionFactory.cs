using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace customauthpg.Services;

public class DbConnectionFactory : IDisposable
{
    private readonly ILogger<DbConnectionFactory> _logger;
    private readonly string _connectionString;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private NpgsqlConnection _connection;
    private bool _disposed;

    public DbConnectionFactory(IConfiguration configuration, ILogger<DbConnectionFactory> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DbConnectionFactory));
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                if (_connection != null)
                {
                    await _connection.DisposeAsync();
                }

                _connection = new NpgsqlConnection(_connectionString);
                try
                {
                    await _connection.OpenAsync();
                    _logger.LogInformation("Database connection opened successfully.");
                }
                catch (NpgsqlException ex)
                {
                    _logger.LogError(ex, "Failed to open database connection.");
                    throw new DatabaseConnectionException("Failed to establish database connection.", ex);
                }
            }
            return _connection;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _semaphore.Wait();
                try
                {
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _logger.LogInformation("Database connection closed and disposed.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while disposing database connection.");
                }
                finally
                {
                    _semaphore.Release();
                }
                _semaphore.Dispose();
            }
            _disposed = true;
        }
    }
}

public class DatabaseConnectionException : Exception
{
    public DatabaseConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}