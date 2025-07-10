using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateModel> _logger;
        private readonly string _apiBaseUrl = "http://localhost:5164/api";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public OrchidDto Orchid { get; set; } = new OrchidDto();

        public SelectList CategorySelectList { get; set; } = default!;

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadCategoriesAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing create orchid page");
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
                    _logger.LogInformation("Adding JWT token to API create request");
                }
                else
                {
                    _logger.LogWarning("No access token found in user claims for create operation");
                }

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Use PropertyNamingPolicy = null to preserve property names as-is
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                };

                // Create object matching the API's expected structure
                var createDto = new
                {
                    OrchidName = Orchid.OrchidName,
                    OrchidDescription = Orchid.OrchidDescription,
                    OrchidUrl = Orchid.OrchidUrl,
                    Price = Orchid.Price,
                    IsNatural = Orchid.IsNatural,
                    CategoryId = Orchid.CategoryId
                };

                var orchidJson = JsonSerializer.Serialize(createDto, jsonOptions);
                var content = new StringContent(orchidJson, Encoding.UTF8, "application/json");

                _logger.LogInformation("Creating new orchid: {Data}", orchidJson);

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/Orchid", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Orchid created successfully: {Response}", responseContent);

                    var createdOrchid = JsonSerializer.Deserialize<OrchidDto>(responseContent, _jsonOptions);
                    StatusMessage = $"Orchid '{createdOrchid?.OrchidName}' was created successfully.";

                    return RedirectToPage("./Index");
                }

                // If we get here, something went wrong with the API call
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API create failed: {StatusCode}, {Content}", response.StatusCode, errorContent);

                ModelState.AddModelError(string.Empty,
                    $"API Error: {response.StatusCode} - {errorContent}");

                await LoadCategoriesAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating orchid");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                await LoadCategoriesAsync();
                return Page();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                var token = User.FindFirst("AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Get categories from API
                var categoriesResponse = await httpClient.GetAsync($"{_apiBaseUrl}/Category");
                _logger.LogInformation("Getting categories from: {Url}", $"{_apiBaseUrl}/Category");

                if (categoriesResponse.IsSuccessStatusCode)
                {
                    var responseContent = await categoriesResponse.Content.ReadAsStringAsync();

                    using (JsonDocument document = JsonDocument.Parse(responseContent))
                    {
                        var root = document.RootElement;
                        List<CategoryDto> categories;

                        if (root.ValueKind == JsonValueKind.Array)
                        {
                            categories = JsonSerializer.Deserialize<List<CategoryDto>>(responseContent, _jsonOptions) ??
                                new List<CategoryDto>();
                        }
                        else if (root.TryGetProperty("$values", out var valuesElement) &&
                                 valuesElement.ValueKind == JsonValueKind.Array)
                        {
                            categories = JsonSerializer.Deserialize<List<CategoryDto>>(
                                valuesElement.GetRawText(), _jsonOptions) ?? new List<CategoryDto>();
                        }
                        else if (root.TryGetProperty("items", out var itemsObject) &&
                                 itemsObject.TryGetProperty("$values", out var itemValues) &&
                                 itemValues.ValueKind == JsonValueKind.Array)
                        {
                            categories = JsonSerializer.Deserialize<List<CategoryDto>>(
                                itemValues.GetRawText(), _jsonOptions) ?? new List<CategoryDto>();
                        }
                        else
                        {
                            _logger.LogWarning("Unknown category response structure");
                            categories = new List<CategoryDto>();
                        }

                        _logger.LogInformation("Loaded {Count} categories from API", categories.Count);
                        CategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                    }
                }
                else
                {
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

        // DTOs for API interaction
        public class OrchidDto
        {
            public int OrchidId { get; set; }
            public string OrchidName { get; set; } = string.Empty;
            public string? OrchidDescription { get; set; }
            public string? OrchidUrl { get; set; }
            public decimal? Price { get; set; }
            public bool? IsNatural { get; set; }
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