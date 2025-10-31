using Blog.Models;
using Microsoft.Data.Sqlite;

namespace Blog.Data
{
    
    public class ArticleRepository : IArticleRepository
    {
        private readonly string _connectionString;

        public ArticleRepository(DatabaseConfig _config)
        {
            _connectionString = _config.DefaultConnectionString ?? throw new ArgumentNullException("Connection string not found");
        }

        /// <summary>
   
        /// </summary>
        public void EnsureCreated()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createArticlesTable = @"
                CREATE TABLE IF NOT EXISTS Articles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AuthorName TEXT NOT NULL,
                    AuthorEmail TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    PublishedDate TEXT NOT NULL
                )";

            var createCommentsTable = @"
                CREATE TABLE IF NOT EXISTS Comments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ArticleId INTEGER NOT NULL,
                    Content TEXT NOT NULL,
                    PublishedDate TEXT NOT NULL,
                    FOREIGN KEY (ArticleId) REFERENCES Articles(Id)
                )";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = createArticlesTable;
                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = createCommentsTable;
                command.ExecuteNonQuery();
            }
        }

        public IEnumerable<Article> GetAll()
        {
            var articles = new List<Article>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT Id, AuthorName, AuthorEmail, Title, Content, PublishedDate FROM Articles ORDER BY PublishedDate DESC";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                articles.Add(new Article
                {
                    Id = reader.GetInt32(0),
                    AuthorName = reader.GetString(1),
                    AuthorEmail = reader.GetString(2),
                    Title = reader.GetString(3),
                    Content = reader.GetString(4),
                    PublishedDate = DateTimeOffset.Parse(reader.GetString(5))
                });
            }

            return articles;
        }

        public IEnumerable<Article> GetByDateRange(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var articles = new List<Article>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT Id, AuthorName, AuthorEmail, Title, Content, PublishedDate 
                FROM Articles 
                WHERE PublishedDate >= @StartDate AND PublishedDate <= @EndDate
                ORDER BY PublishedDate DESC";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.ToString("o"));
            command.Parameters.AddWithValue("@EndDate", endDate.ToString("o"));

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                articles.Add(new Article
                {
                    Id = reader.GetInt32(0),
                    AuthorName = reader.GetString(1),
                    AuthorEmail = reader.GetString(2),
                    Title = reader.GetString(3),
                    Content = reader.GetString(4),
                    PublishedDate = DateTimeOffset.Parse(reader.GetString(5))
                });
            }

            return articles;
        }

        public Article? GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT Id, AuthorName, AuthorEmail, Title, Content, PublishedDate FROM Articles WHERE Id = @Id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Article
                {
                    Id = reader.GetInt32(0),
                    AuthorName = reader.GetString(1),
                    AuthorEmail = reader.GetString(2),
                    Title = reader.GetString(3),
                    Content = reader.GetString(4),
                    PublishedDate = DateTimeOffset.Parse(reader.GetString(5))
                };
            }

            return null;
        }

        public Article Create(Article article)
        {
            if (article == null)
                throw new ArgumentNullException(nameof(article));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                INSERT INTO Articles (AuthorName, AuthorEmail, Title, Content, PublishedDate)
                VALUES (@AuthorName, @AuthorEmail, @Title, @Content, @PublishedDate);
                SELECT last_insert_rowid();";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@AuthorName", article.AuthorName);
            command.Parameters.AddWithValue("@AuthorEmail", article.AuthorEmail);
            command.Parameters.AddWithValue("@Title", article.Title);
            command.Parameters.AddWithValue("@Content", article.Content);
            command.Parameters.AddWithValue("@PublishedDate", article.PublishedDate.ToString("o"));

            var newId = Convert.ToInt32(command.ExecuteScalar());
            article.Id = newId;

            return article;
        }

        public void AddComment(Comment comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            // Verify that the article exists
            var article = GetById(comment.ArticleId);
            if (article == null)
                throw new ArgumentException("No article exists with the specified ID.", nameof(comment));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                INSERT INTO Comments (ArticleId, Content, PublishedDate)
                VALUES (@ArticleId, @Content, @PublishedDate)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@ArticleId", comment.ArticleId);
            command.Parameters.AddWithValue("@Content", comment.Content);
            command.Parameters.AddWithValue("@PublishedDate", comment.PublishedDate.ToString("o"));

            command.ExecuteNonQuery();
        }

        public IEnumerable<Comment> GetCommentsByArticleId(int articleId)
        {
            var comments = new List<Comment>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT ArticleId, Content, PublishedDate 
                FROM Comments 
                WHERE ArticleId = @ArticleId
                ORDER BY PublishedDate ASC";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@ArticleId", articleId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                comments.Add(new Comment
                {
                    ArticleId = reader.GetInt32(0),
                    Content = reader.GetString(1),
                    PublishedDate = DateTimeOffset.Parse(reader.GetString(2))
                });
            }

            return comments;
        }
    }
}