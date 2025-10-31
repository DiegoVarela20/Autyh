using Blog.Data;
using Blog.Models;
using Blog.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly PasswordHasher _passwordHasher;
        private readonly SessionService _sessionService;
        private const string SessionCookieName = "BlogSession";

        public AuthController(IAuthRepository authRepository, PasswordHasher passwordHasher, SessionService sessionService)
        {
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
            _sessionService = sessionService;
        }

        // GET: Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            // If already logged in, redirect to home
            if (IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Articles");
            }

            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username already exists
            if (_authRepository.UsernameExists(model.Username))
            {
                ModelState.AddModelError("Username", "This username is already taken");
                return View(model);
            }

            // Check if email already exists
            if (_authRepository.EmailExists(model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered");
                return View(model);
            }

            // Hash the password
            string passwordHash = _passwordHasher.HashPassword(model.Password, out string salt);

            // Create the user
            var user = new User
            {
                Username = model.Username,
                Name = model.Name,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth,
                PasswordHash = passwordHash,
                PasswordSalt = salt,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _authRepository.CreateUser(user);

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to home
            if (IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Articles");
            }

            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get user by username
            var user = _authRepository.GetUserByUsername(model.Username);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Create session
            var sessionId = _sessionService.GenerateSessionId();
            var expiresAt = _sessionService.GetExpirationTime();

            var session = new Session
            {
                SessionId = sessionId,
                UserId = user.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                LastActivityAt = DateTimeOffset.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true
            };

            _authRepository.CreateSession(session);

            // Set cookie with session ID
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Prevent JavaScript access
                Secure = true, // Only send over HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt
            };

            Response.Cookies.Append(SessionCookieName, sessionId, cookieOptions);

            return RedirectToAction("Dashboard", "Auth");
        }

        // GET: Auth/Dashboard (Protected page)
        [HttpGet]
        public IActionResult Dashboard()
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(user);
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            if (Request.Cookies.TryGetValue(SessionCookieName, out var sessionId))
            {
                _authRepository.InvalidateSession(sessionId);
                Response.Cookies.Delete(SessionCookieName);
            }

            return RedirectToAction("Index", "Articles");
        }

        // Helper methods
        private bool IsUserAuthenticated()
        {
            if (!Request.Cookies.TryGetValue(SessionCookieName, out var sessionId))
            {
                return false;
            }

            var session = _authRepository.GetSessionById(sessionId);
            if (session == null || !session.IsActive)
            {
                return false;
            }

            // Check if session is expired
            if (_sessionService.IsSessionExpired(session.ExpiresAt))
            {
                _authRepository.InvalidateSession(sessionId);
                return false;
            }

            // Update last activity
            var newExpiresAt = _sessionService.GetExpirationTime();
            _authRepository.UpdateSessionActivity(sessionId, newExpiresAt);

            // Update cookie expiration
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = newExpiresAt
            };
            Response.Cookies.Append(SessionCookieName, sessionId, cookieOptions);

            return true;
        }

        private User? GetCurrentUser()
        {
            if (!Request.Cookies.TryGetValue(SessionCookieName, out var sessionId))
            {
                return null;
            }

            var session = _authRepository.GetSessionById(sessionId);
            if (session == null || !session.IsActive)
            {
                return null;
            }

            // Check if session is expired
            if (_sessionService.IsSessionExpired(session.ExpiresAt))
            {
                _authRepository.InvalidateSession(sessionId);
                return null;
            }

            // Update last activity
            var newExpiresAt = _sessionService.GetExpirationTime();
            _authRepository.UpdateSessionActivity(sessionId, newExpiresAt);

            // Update cookie expiration
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = newExpiresAt
            };
            Response.Cookies.Append(SessionCookieName, sessionId, cookieOptions);

            return _authRepository.GetUserById(session.UserId);
        }
    }
}