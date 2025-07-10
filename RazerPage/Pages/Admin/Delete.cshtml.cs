using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace RazerPage.Pages.Admin
{
    [Authorize(Roles = "1")]
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DeleteModel> _logger;
        private readonly string _apiBaseUrl = "http://localhost:5164/api";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public OrchidDto Orchid { get; set; } = default!;

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // Get token from user claims
                var token = User.FindFirst("AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("Adding JWT token to API request for Delete page");
                }
                else
                {
                    _logger.LogWarning("No access token found in user claims for Delete page");
                }

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Get orchid details to display for confirmation
                var response = await httpClient.GetAsync($"{_apiBaseUrl}/Orchid/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned non-success status code: {StatusCode}", response.StatusCode);
                    return NotFound();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response: {Response}", responseContent);

                Orchid = JsonSerializer.Deserialize<OrchidDto>(responseContent, _jsonOptions) ??
                    throw new InvalidOperationException("Failed to deserialize orchid data");

                return Page();
            }
            catch (HttpRequestException ex)
            {
                // Handle API connectivity issues
                _logger.LogError(ex, "Error connecting to API for orchid data");
                ModelState.AddModelError(string.Empty, "Unable to connect to the API. Please try again later.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orchid data");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // Get token from user claims
                var token = User.FindFirst("AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("Adding JWT token to API delete request");
                }
                else
                {
                    _logger.LogWarning("No access token found in user claims for delete");
                }

                _logger.LogInformation("Deleting orchid with ID: {OrchidId}", id);

                // Send the DELETE request to the API
                var response = await httpClient.DeleteAsync($"{_apiBaseUrl}/Orchid/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Orchid {OrchidId} deleted successfully", id);
                    StatusMessage = $"Orchid #{id} was successfully deleted.";
                    return RedirectToPage("./Index");
                }

                // If we get here, something went wrong with the API call
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API delete failed: {StatusCode}, {Content}", response.StatusCode, errorContent);

                ModelState.AddModelError(string.Empty,
                    $"API Error: {response.StatusCode} - {errorContent}");

                // Get the orchid details again to redisplay the page
                var getResponse = await httpClient.GetAsync($"{_apiBaseUrl}/Orchid/{id}");
                if (getResponse.IsSuccessStatusCode)
                {
                    var getContent = await getResponse.Content.ReadAsStringAsync();
                    Orchid = JsonSerializer.Deserialize<OrchidDto>(getContent, _jsonOptions);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting orchid");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");

                // Try to get the orchid details again
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var token = User.FindFirst("AccessToken")?.Value;
                    if (!string.IsNullOrEmpty(token))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    var getResponse = await httpClient.GetAsync($"{_apiBaseUrl}/Orchid/{id}");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        var getContent = await getResponse.Content.ReadAsStringAsync();
                        Orchid = JsonSerializer.Deserialize<OrchidDto>(getContent, _jsonOptions);
                    }
                }
                catch
                {
                    // Ignore errors when trying to get the orchid details again
                }

                return Page();
            }
        }

        // Local DTO class to match the API response
        public class OrchidDto
        {
            public int OrchidId { get; set; }
            public bool? IsNatural { get; set; }
            public string? OrchidDescription { get; set; }
            public string? OrchidName { get; set; }
            public string? OrchidUrl { get; set; }
            public decimal? Price { get; set; }
            public int? CategoryId { get; set; }
            public string? CategoryName { get; set; }
        }
    }
}