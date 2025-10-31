using Blog.Models;
using Microsoft.Data.Sqlite;

namespace Blog.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(DatabaseConfig config)
        {
            _connectionString = config.DefaultConnectionString ??
                throw new ArgumentNullException("Connection string not found");
        }

        public void EnsureCreated()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createUsersTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL UNIQUE,
                    DateOfBirth TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    PasswordSalt TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                )";

            var createSessionsTable = @"
                CREATE TABLE IF NOT EXISTS Sessions (
                    SessionId TEXT PRIMARY KEY,
                    UserId INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    LastActivityAt TEXT NOT NULL,
                    ExpiresAt TEXT NOT NULL,
                    IsActive INTEGER NOT NULL,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                )";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = createUsersTable;
                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = createSessionsTable;
                command.ExecuteNonQuery();
            }
        }

        public User CreateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                INSERT INTO Users (Username, Name, Email, DateOfBirth, PasswordHash, PasswordSalt, CreatedAt)
                VALUES (@Username, @Name, @Email, @DateOfBirth, @PasswordHash, @PasswordSalt, @CreatedAt);
                SELECT last_insert_rowid();";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Name", user.Name);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);
            command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt.ToString("o"));

            var newId = Convert.ToInt32(command.ExecuteScalar());
            user.Id = newId;

            return user;
        }

        public User? GetUserByUsername(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT Id, Username, Name, Email, DateOfBirth, PasswordHash, PasswordSalt, CreatedAt 
                FROM Users 
                WHERE Username = @Username";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Name = reader.GetString(2),
                    Email = reader.GetString(3),
                    DateOfBirth = DateTime.Parse(reader.GetString(4)),
                    PasswordHash = reader.GetString(5),
                    PasswordSalt = reader.GetString(6),
                    CreatedAt = DateTimeOffset.Parse(reader.GetString(7))
                };
            }

            return null;
        }

        public User? GetUserById(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT Id, Username, Name, Email, DateOfBirth, PasswordHash, PasswordSalt, CreatedAt 
                FROM Users 
                WHERE Id = @UserId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Name = reader.GetString(2),
                    Email = reader.GetString(3),
                    DateOfBirth = DateTime.Parse(reader.GetString(4)),
                    PasswordHash = reader.GetString(5),
                    PasswordSalt = reader.GetString(6),
                    CreatedAt = DateTimeOffset.Parse(reader.GetString(7))
                };
            }

            return null;
        }

        public bool UsernameExists(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        public bool EmailExists(string email)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        public Session CreateSession(Session session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                INSERT INTO Sessions (SessionId, UserId, CreatedAt, LastActivityAt, ExpiresAt, IsActive)
                VALUES (@SessionId, @UserId, @CreatedAt, @LastActivityAt, @ExpiresAt, @IsActive)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@SessionId", session.SessionId);
            command.Parameters.AddWithValue("@UserId", session.UserId);
            command.Parameters.AddWithValue("@CreatedAt", session.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("@LastActivityAt", session.LastActivityAt.ToString("o"));
            command.Parameters.AddWithValue("@ExpiresAt", session.ExpiresAt.ToString("o"));
            command.Parameters.AddWithValue("@IsActive", session.IsActive ? 1 : 0);

            command.ExecuteNonQuery();

            return session;
        }

        public Session? GetSessionById(string sessionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT SessionId, UserId, CreatedAt, LastActivityAt, ExpiresAt, IsActive 
                FROM Sessions 
                WHERE SessionId = @SessionId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@SessionId", sessionId);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Session
                {
                    SessionId = reader.GetString(0),
                    UserId = reader.GetInt32(1),
                    CreatedAt = DateTimeOffset.Parse(reader.GetString(2)),
                    LastActivityAt = DateTimeOffset.Parse(reader.GetString(3)),
                    ExpiresAt = DateTimeOffset.Parse(reader.GetString(4)),
                    IsActive = reader.GetInt32(5) == 1
                };
            }

            return null;
        }

        public void UpdateSessionActivity(string sessionId, DateTimeOffset expiresAt)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                UPDATE Sessions 
                SET LastActivityAt = @LastActivityAt, ExpiresAt = @ExpiresAt
                WHERE SessionId = @SessionId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@SessionId", sessionId);
            command.Parameters.AddWithValue("@LastActivityAt", DateTimeOffset.UtcNow.ToString("o"));
            command.Parameters.AddWithValue("@ExpiresAt", expiresAt.ToString("o"));

            command.ExecuteNonQuery();
        }

        public void InvalidateSession(string sessionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                UPDATE Sessions 
                SET IsActive = 0 
                WHERE SessionId = @SessionId";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@SessionId", sessionId);

            command.ExecuteNonQuery();
        }

        public void CleanupExpiredSessions()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                DELETE FROM Sessions 
                WHERE ExpiresAt < @Now OR IsActive = 0";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Now", DateTimeOffset.UtcNow.ToString("o"));

            command.ExecuteNonQuery();
        }
    }
}