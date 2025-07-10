using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazerPage.Pages.User
{
    [Authorize(Roles = "2")]
    public class CartModel : PageModel
    {
        private readonly ILogger<CartModel> _logger;

        public CartModel(ILogger<CartModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Cart is managed client-side using cookies
            _logger.LogInformation("Cart page visited");
        }
    }
}