using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Blog.Services
{
    /// <summary>
    /// Service for securely hashing and verifying passwords
    /// </summary>
    public class PasswordHasher
    {
        /// <summary>
        /// Hashes a password with a randomly generated salt
        /// </summary>
        /// <param name="password">The plain text password</param>
        /// <param name="salt">Output parameter containing the generated salt</param>
        /// <returns>The hashed password as a Base64 string</returns>
        public string HashPassword(string password, out string salt)
        {
            // Generate a 128-bit salt using cryptographically strong random bytes
            byte[] saltBytes = RandomNumberGenerator.GetBytes(128 / 8);
            salt = Convert.ToBase64String(saltBytes);

            // Derive a 256-bit subkey using HMACSHA256 with 100,000 iterations
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        /// <summary>
        /// Verifies a password against a stored hash and salt
        /// </summary>
        /// <param name="password">The plain text password to verify</param>
        /// <param name="storedHash">The stored password hash</param>
        /// <param name="storedSalt">The stored salt</param>
        /// <returns>True if the password matches, false otherwise</returns>
        public bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            // Hash the provided password with the stored salt
            string hashToVerify = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            // Compare the hashes
            return hashToVerify == storedHash;
        }
    }
}