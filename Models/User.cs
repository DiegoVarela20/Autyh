using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    /// <summary>
    /// Represents a registered user in the blog system
    /// </summary>
    public class User
    {
        /// <summary>
        /// The unique identifier for the user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The unique username for authentication
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        /// <summary>
        /// The user's full name
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        /// <summary>
        /// The user's email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// The user's date of birth
        /// </summary>
        [Required(ErrorMessage = "Date of birth is required")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// The hashed password (never store plain text passwords)
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// The salt used for password hashing
        /// </summary>
        [Required]
        public string PasswordSalt { get; set; }

        /// <summary>
        /// When the user account was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
    }
}