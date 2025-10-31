namespace Blog.Models
{
    /// <summary>
    /// Represents an active user session
    /// </summary>
    public class Session
    {
        /// <summary>
        /// The unique session identifier (128-bit random ID)
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// The ID of the user who owns this session
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// When the session was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// The last time this session was used
        /// </summary>
        public DateTimeOffset LastActivityAt { get; set; }

        /// <summary>
        /// When the session expires
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Whether the session is still valid
        /// </summary>
        public bool IsActive { get; set; }
    }
}