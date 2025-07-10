using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace RazerPage.Pages;
public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(ILogger<LoginModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public LoginInputModel LoginInput { get; set; }

    public string ErrorMessage { get; set; }

    // Add property to store the token for client-side access
    public string JwtToken { get; private set; }

    public class LoginInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = "http://localhost:5164/User/GenerateToken";

            var loginData = new
            {
                Email = LoginInput.Email,
                Password = LoginInput.Password
            };

            var response = await client.PostAsJsonAsync(apiUrl, loginData);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw response: {JsonResponse}", jsonResponse);

                var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();

                if (authResult != null && !string.IsNullOrEmpty(authResult.Token))
                {
                    _logger.LogInformation("Auth result: Token={Token}, Role={Role}, Success={Success}",
                        "Token present", authResult.Role, authResult.Success);

                    // Store token for use in JavaScript
                    JwtToken = authResult.Token;

                    // Create the claims for the user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, LoginInput.Email),
                        new Claim("AccessToken", authResult.Token)
                    };

                    if (authResult.Role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, authResult.Role));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = LoginInput.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    _logger.LogInformation("User {Email} logged in with role {Role}", LoginInput.Email, authResult.Role);

                    // Store JWT token in TempData for script access
                    TempData["JwtToken"] = authResult.Token;

                    string roleValue = authResult.Role?.Trim();
                    _logger.LogInformation("Checking role: '{RoleValue}'", roleValue);

                    if (roleValue == "1" ||
                        string.Equals(roleValue, "admin", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Redirecting to AdminOrder page for admin user");
                        return RedirectToPage("Admin/AdminOrder");
                    }
                    if (roleValue == "2" ||
                      string.Equals(roleValue, "user", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Redirecting to User/Index page for regular user");
                        return RedirectToPage("User/Index");
                    }

                    _logger.LogInformation("Redirecting to home page for non-admin user");
                    return LocalRedirect("~/");
                }
                else
                {
                    _logger.LogWarning("Auth result invalid: Token is empty or auth result is null");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login attempt");
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again later.");
            return Page();
        }
    }

    private class AuthResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}