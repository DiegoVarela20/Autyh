using System.Security.Cryptography;

namespace Blog.Services
{
    /// <summary>
    /// Service for managing user sessions
    /// </summary>
    public class SessionService
    {
        private readonly int _sessionDurationMinutes;

        public SessionService(int sessionDurationMinutes = 5)
        {
            _sessionDurationMinutes = sessionDurationMinutes;
        }

        /// <summary>
        /// Generates a cryptographically secure 128-bit session ID
        /// </summary>
        /// <returns>A Base64-encoded session ID</returns>
        public string GenerateSessionId()
        {
            // Generate a 128-bit (16 bytes) random ID using cryptographically secure random bytes
            byte[] randomBytes = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Gets the expiration time for a new session (current time + session duration)
        /// </summary>
        /// <returns>The expiration timestamp</returns>
        public DateTimeOffset GetExpirationTime()
        {
            return DateTimeOffset.UtcNow.AddMinutes(_sessionDurationMinutes);
        }

        /// <summary>
        /// Checks if a session has expired
        /// </summary>
        /// <param name="expiresAt">The expiration timestamp of the session</param>
        /// <returns>True if the session has expired, false otherwise</returns>
        public bool IsSessionExpired(DateTimeOffset expiresAt)
        {
            return DateTimeOffset.UtcNow >= expiresAt;
        }
    }
}