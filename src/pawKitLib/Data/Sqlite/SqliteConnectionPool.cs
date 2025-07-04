using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;

namespace PawKitLib.Data.Sqlite;

/// <summary>
/// A simple connection pool for SQLite connections to improve performance under high load.
/// </summary>
public sealed class SqliteConnectionPool : IDisposable
{
    private readonly string _connectionString;
    private readonly ConcurrentQueue<SqliteConnection> _connections = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxPoolSize;
    private volatile int _currentPoolSize;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the SqliteConnectionPool class.
    /// </summary>
    /// <param name="connectionString">The connection string for SQLite connections.</param>
    /// <param name="maxPoolSize">The maximum number of connections to pool. Default is 10.</param>
    public SqliteConnectionPool(string connectionString, int maxPoolSize = 10)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _maxPoolSize = maxPoolSize > 0 ? maxPoolSize : throw new ArgumentOutOfRangeException(nameof(maxPoolSize), "Max pool size must be greater than zero.");
        _semaphore = new SemaphoreSlim(_maxPoolSize, _maxPoolSize);
    }

    /// <summary>
    /// Gets a connection from the pool or creates a new one if the pool is empty.
    /// </summary>
    /// <returns>A pooled SQLite connection.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the pool has been disposed.</exception>
    public async Task<PooledConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SqliteConnectionPool));

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            SqliteConnection? connection = null;

            // Try to get a connection from the pool
            while (_connections.TryDequeue(out var pooledConnection))
            {
                // Check if the connection is still valid
                if (pooledConnection.State == System.Data.ConnectionState.Open)
                {
                    connection = pooledConnection;
                    break;
                }
                else
                {
                    // Connection is invalid, dispose it and decrement count
                    pooledConnection.Dispose();
                    Interlocked.Decrement(ref _currentPoolSize);
                }
            }

            // If no valid connection was found, create a new one
            if (connection == null)
            {
                connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                Interlocked.Increment(ref _currentPoolSize);
            }

            return new PooledConnection(connection, this);
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    /// <summary>
    /// Gets a connection from the pool synchronously.
    /// </summary>
    /// <returns>A pooled SQLite connection.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the pool has been disposed.</exception>
    public PooledConnection GetConnection()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SqliteConnectionPool));

        _semaphore.Wait();

        try
        {
            SqliteConnection? connection = null;

            // Try to get a connection from the pool
            while (_connections.TryDequeue(out var pooledConnection))
            {
                // Check if the connection is still valid
                if (pooledConnection.State == System.Data.ConnectionState.Open)
                {
                    connection = pooledConnection;
                    break;
                }
                else
                {
                    // Connection is invalid, dispose it and decrement count
                    pooledConnection.Dispose();
                    Interlocked.Decrement(ref _currentPoolSize);
                }
            }

            // If no valid connection was found, create a new one
            if (connection == null)
            {
                connection = new SqliteConnection(_connectionString);
                connection.Open();
                Interlocked.Increment(ref _currentPoolSize);
            }

            return new PooledConnection(connection, this);
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    /// <summary>
    /// Returns a connection to the pool.
    /// </summary>
    /// <param name="connection">The connection to return to the pool.</param>
    internal void ReturnConnection(SqliteConnection connection)
    {
        if (_disposed || connection == null)
        {
            connection?.Dispose();
            _semaphore.Release();
            return;
        }

        try
        {
            // Only return healthy connections to the pool
            if (connection.State == System.Data.ConnectionState.Open && _currentPoolSize <= _maxPoolSize)
            {
                _connections.Enqueue(connection);
            }
            else
            {
                connection.Dispose();
                Interlocked.Decrement(ref _currentPoolSize);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Disposes all connections in the pool.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Dispose all connections in the pool
        while (_connections.TryDequeue(out var connection))
        {
            try
            {
                connection.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        _semaphore.Dispose();
    }
}

/// <summary>
/// Represents a pooled SQLite connection that automatically returns to the pool when disposed.
/// </summary>
public class PooledConnection : IDisposable
{
    private readonly SqliteConnectionPool _pool;
    private SqliteConnection? _connection;
    private bool _disposed;

    /// <summary>
    /// Gets the underlying SQLite connection.
    /// </summary>
    public SqliteConnection Connection => _connection ?? throw new ObjectDisposedException(nameof(PooledConnection));

    /// <summary>
    /// Initializes a new instance of the PooledConnection class.
    /// </summary>
    /// <param name="connection">The SQLite connection.</param>
    /// <param name="pool">The connection pool.</param>
    public PooledConnection(SqliteConnection connection, SqliteConnectionPool pool)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }

    /// <summary>
    /// Returns the connection to the pool.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_connection != null)
        {
            _pool.ReturnConnection(_connection);
            _connection = null;
        }
    }
}