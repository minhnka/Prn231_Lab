using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace RazerPage.Pages.Admin
{
    [Authorize(Roles = "1")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;
        private readonly string _apiBaseUrl = "http://localhost:5164/api";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public OrchidDto Orchid { get; set; } = default!;

        public SelectList CategorySelectList { get; set; } = default!;

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
                    _logger.LogInformation("Adding JWT token to API request for Edit page");
                }
                else
                {
                    _logger.LogWarning("No access token found in user claims for Edit page");
                }

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Get orchid details
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

                // Get categories for dropdown
                await LoadCategoriesAsync(httpClient);

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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // Get token from user claims
                var token = User.FindFirst("AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("Adding JWT token to API update request");
                }
                else
                {
                    _logger.LogWarning("No access token found in user claims for update");
                }

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var orchidJson = JsonSerializer.Serialize(Orchid, _jsonOptions);
                var content = new StringContent(orchidJson, Encoding.UTF8, "application/json");

                _logger.LogInformation("Updating orchid: {OrchidId} with data: {Data}", Orchid.OrchidId, orchidJson);

                // Fixed: Remove the ID from the URL path
                var response = await httpClient.PutAsync($"{_apiBaseUrl}/Orchid", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Orchid {OrchidId} updated successfully", Orchid.OrchidId);
                    return RedirectToPage("./Index");
                }

                // If we get here, something went wrong with the API call
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API update failed: {StatusCode}, {Content}", response.StatusCode, errorContent);

                ModelState.AddModelError(string.Empty,
                    $"API Error: {response.StatusCode} - {errorContent}");

                await LoadCategoriesAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating orchid");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                await LoadCategoriesAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateOrchidAsync(int id, [FromBody] OrchidDto updatedOrchid)
        {
            if (id != updatedOrchid.OrchidId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // Get token from user claims
                var token = User.FindFirst("AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("Adding JWT token to API update request");
                }
                else
                {
                    _logger.LogWarning("No access token found in user claims for update");
                }

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Create an UpdateOrchidDto that exactly matches the controller's expected format
                var updateDto = new 
                {
                    OrchidId = updatedOrchid.OrchidId,
                    OrchidName = updatedOrchid.OrchidName,
                    OrchidDescription = updatedOrchid.OrchidDescription,
                    OrchidUrl = updatedOrchid.OrchidUrl,
                    Price = updatedOrchid.Price,
                    IsNatural = updatedOrchid.IsNatural,
                    CategoryId = updatedOrchid.CategoryId
                };

                // Use camelCase = false to ensure property names match exactly what the API expects
                var jsonOptions = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = null // Preserves the casing of property names
                };
                
                var orchidJson = JsonSerializer.Serialize(updateDto, jsonOptions);
                var content = new StringContent(orchidJson, Encoding.UTF8, "application/json");

                _logger.LogInformation("Updating orchid via API: {OrchidId}, JSON: {Json}", updatedOrchid.OrchidId, orchidJson);

                // Endpoint should be just /api/Orchid with no ID in the URL
                var response = await httpClient.PutAsync($"{_apiBaseUrl}/Orchid", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Orchid updated successfully: {Response}", responseContent);
                    return new JsonResult(new { success = true, message = "Orchid updated successfully" });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API update failed: {StatusCode}, {Content}", response.StatusCode, errorContent);
                
                return StatusCode((int)response.StatusCode, new { message = errorContent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating orchid");
                return StatusCode(500, new { message = $"Internal error: {ex.Message}" });
            }
        }

        private async Task LoadCategoriesAsync(HttpClient? client = null)
        {
            try
            {
                var httpClient = client ?? _httpClientFactory.CreateClient();

                // If client was not provided, we need to set authorization header
                if (client == null)
                {
                    var token = User.FindFirst("AccessToken")?.Value;
                    if (!string.IsNullOrEmpty(token))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }

                // Get categories from API - fixed URL to use the correct endpoint
                var categoriesResponse = await httpClient.GetAsync($"{_apiBaseUrl}/Category");
                _logger.LogInformation("Getting categories from: {Url}", $"{_apiBaseUrl}/Category");

                if (categoriesResponse.IsSuccessStatusCode)
                {
                    var responseContent = await categoriesResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation("Category API Response: {Response}", responseContent);

                    // Try to parse the response to check the structure
                    using (JsonDocument document = JsonDocument.Parse(responseContent))
                    {
                        var root = document.RootElement;
                        List<CategoryDto> categories;

                        // Check if the response is an array directly or has a nested structure
                        if (root.ValueKind == JsonValueKind.Array)
                        {
                            // Direct array of categories
                            categories = JsonSerializer.Deserialize<List<CategoryDto>>(responseContent, _jsonOptions) ??
                                new List<CategoryDto>();
                        }
                        else if (root.TryGetProperty("$values", out var valuesElement) &&
                                 valuesElement.ValueKind == JsonValueKind.Array)
                        {
                            // Nested array in $values property
                            categories = JsonSerializer.Deserialize<List<CategoryDto>>(
                                valuesElement.GetRawText(), _jsonOptions) ?? new List<CategoryDto>();
                        }
                        else if (root.TryGetProperty("items", out var itemsObject) &&
                                 itemsObject.TryGetProperty("$values", out var itemValues) &&
                                 itemValues.ValueKind == JsonValueKind.Array)
                        {
                            // Nested array in items.$values property (pagination structure)
                            categories = JsonSerializer.Deserialize<List<CategoryDto>>(
                                itemValues.GetRawText(), _jsonOptions) ?? new List<CategoryDto>();
                        }
                        else
                        {
                            // Fallback if structure is unknown
                            _logger.LogWarning("Unknown category response structure");
                            categories = new List<CategoryDto>();
                        }

                        _logger.LogInformation("Loaded {Count} categories from API", categories.Count);
                        CategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                    }
                }
                else
                {
                    // Fallback to empty list if categories can't be loaded
                    _logger.LogWarning("Failed to load categories: {StatusCode}", categoriesResponse.StatusCode);
                    CategorySelectList = new SelectList(new List<CategoryDto>(), "CategoryId", "CategoryName");
                    ModelState.AddModelError(string.Empty, "Unable to load categories from API");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                CategorySelectList = new SelectList(new List<CategoryDto>(), "CategoryId", "CategoryName");
                ModelState.AddModelError(string.Empty, $"Error loading categories: {ex.Message}");
            }
        }

        // Local DTO classes to match the API responses
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

        public class CategoryDto
        {
            public int CategoryId { get; set; }
            public string? CategoryName { get; set; }
        }
    }
}   