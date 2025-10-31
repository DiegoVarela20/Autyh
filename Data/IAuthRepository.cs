using Blog.Models;

namespace Blog.Data
{
    public interface IAuthRepository
    {
        // User operations
        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        User CreateUser(User user);

        /// <summary>
        /// Gets a user by their username
        /// </summary>
        User? GetUserByUsername(string username);

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        User? GetUserById(int userId);

        /// <summary>
        /// Checks if a username already exists
        /// </summary>
        bool UsernameExists(string username);

        /// <summary>
        /// Checks if an email already exists
        /// </summary>
        bool EmailExists(string email);

        // Session operations
        /// <summary>
        /// Creates a new session for a user
        /// </summary>
        Session CreateSession(Session session);

        /// <summary>
        /// Gets a session by its ID
        /// </summary>
        Session? GetSessionById(string sessionId);

        /// <summary>
        /// Updates the last activity time of a session
        /// </summary>
        void UpdateSessionActivity(string sessionId, DateTimeOffset expiresAt);

        /// <summary>
        /// Invalidates a session (logout)
        /// </summary>
        void InvalidateSession(string sessionId);

        /// <summary>
        /// Removes expired sessions from the database
        /// </summary>
        void CleanupExpiredSessions();
    }
}