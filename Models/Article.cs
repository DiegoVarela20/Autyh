using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    /// <summary>
    /// Represents a blog article
    /// </summary>
    public class Article
    {
        /// <summary>
        /// The unique identifier for the article. Assigned at creation.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the author who wrote the article.
        /// </summary>
        [Required(ErrorMessage = "Author name is required")]
        [Display(Name = "Author Name")]
        public string AuthorName { get; set; }

        /// <summary>
        /// The email of the author who wrote the article.
        /// </summary>
        [Required(ErrorMessage = "Author email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Author Email")]
        public string AuthorEmail { get; set; }

        /// <summary>
        /// The title of the article. Specified by the user.
        /// It is limited to 100 characters.
        /// </summary>
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// The full content of the article. 
        /// </summary>
        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// Represents the moment the article was published
        /// </summary>
        [Display(Name = "Published Date")]
        public DateTimeOffset PublishedDate { get; set; }
    }
}