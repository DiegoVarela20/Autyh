using Blog.Data;
using Blog.Services;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Get database configuration
            var databaseConfig = builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>();

            // Register Article Repository
            if (databaseConfig!.UseInMemoryDatabase)
            {
                builder.Services.AddSingleton<IArticleRepository, MemoryArticleRepository>();
            }
            else
            {
                builder.Services.AddSingleton<IArticleRepository>(services =>
                {
                    var repository = new ArticleRepository(databaseConfig);
                    repository.EnsureCreated();
                    return repository;
                });
            }

            // Register Auth Repository (always use database for auth, never in-memory)
            builder.Services.AddScoped<IAuthRepository>(services =>
            {
                var repository = new AuthRepository(databaseConfig);
                repository.EnsureCreated();
                return repository;
            });

            // Register authentication services
            builder.Services.AddSingleton<PasswordHasher>();
            builder.Services.AddSingleton<SessionService>(sp =>
                new SessionService(sessionDurationMinutes: 5));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}