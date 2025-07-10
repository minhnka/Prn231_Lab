using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace RazerPage.Pages.Admin
{
    [Authorize(Roles = "1")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;
        private const string OrchidApiBaseUrl = "http://localhost:5164/api/Orchid";

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public IList<OrchidViewModel> Orchid { get; set; } = new List<OrchidViewModel>();
        public int TotalCount { get; set; }
        public int PageCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "OrchidId";

        [BindProperty(SupportsGet = true)]
        public bool Ascending { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsNatural { get; set; }

        public List<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "OrchidId", Text = "ID" },
            new SelectListItem { Value = "OrchidName", Text = "Name" },
            new SelectListItem { Value = "Price", Text = "Price" },
            new SelectListItem { Value = "CategoryName", Text = "Category" }
        };

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Get token from user claims
                var token = User.FindFirst("AccessToken")?.Value;

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("Adding JWT token to API request");
                }
                else
                {
                    _logger.LogWarning("No access token found in user claims");
                }

                // Validate page parameters
                if (CurrentPage < 1) CurrentPage = 1;
                if (PageSize < 1) PageSize = 10;
                if (PageSize > 100) PageSize = 100;

                // Build the query string with all filter parameters
                var apiUrl = $"{OrchidApiBaseUrl}?page={CurrentPage}&pageSize={PageSize}";

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                    apiUrl += $"&searchTerm={Uri.EscapeDataString(SearchTerm)}";

                apiUrl += $"&sortBy={SortBy}&ascending={Ascending}";

                if (CategoryId.HasValue)
                    apiUrl += $"&categoryId={CategoryId}";

                if (MinPrice.HasValue)
                    apiUrl += $"&minPrice={MinPrice}";

                if (MaxPrice.HasValue)
                    apiUrl += $"&maxPrice={MaxPrice}";

                if (IsNatural.HasValue)
                    apiUrl += $"&isNatural={IsNatural}";

                _logger.LogInformation("Calling API with URL: {Url}", apiUrl);

                // Get the raw HTTP response to inspect the actual JSON structure
                var response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response: {Response}", jsonContent);

                // Use JsonDocument to examine the structure first
                using (JsonDocument document = JsonDocument.Parse(jsonContent))
                {
                    // Try to extract the data based on the actual JSON structure
                    var root = document.RootElement;

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // Check for items.$values structure (nested array)
                    if (root.TryGetProperty("items", out var itemsObject) &&
                        itemsObject.TryGetProperty("$values", out var valuesElement) &&
                        valuesElement.ValueKind == JsonValueKind.Array)
                    {
                        // Deserialize the $values array directly
                        var json = valuesElement.GetRawText();
                        Orchid = JsonSerializer.Deserialize<List<OrchidViewModel>>(json, options) ?? new List<OrchidViewModel>();

                        // Get paging information
                        if (root.TryGetProperty("totalCount", out var totalCountElement))
                            TotalCount = totalCountElement.GetInt32();

                        if (root.TryGetProperty("page", out var pageElement))
                            CurrentPage = pageElement.GetInt32();

                        if (root.TryGetProperty("pageSize", out var pageSizeElement))
                            PageSize = pageSizeElement.GetInt32();

                        if (root.TryGetProperty("pageCount", out var pageCountElement))
                            PageCount = pageCountElement.GetInt32();

                        _logger.LogInformation("Successfully retrieved {Count} orchids from API", Orchid.Count);
                    }
                    else
                    {
                        _logger.LogWarning("Unexpected JSON structure. Items.$values array not found");
                        Orchid = new List<OrchidViewModel>();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error connecting to Orchid API");
                Orchid = new List<OrchidViewModel>();
                ModelState.AddModelError(string.Empty, $"Error connecting to API: {ex.Message}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: {Message}", ex.Message);
                Orchid = new List<OrchidViewModel>();
                ModelState.AddModelError(string.Empty, "Error processing API response. The data format is unexpected.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from Orchid API");
                Orchid = new List<OrchidViewModel>();
                ModelState.AddModelError(string.Empty, $"Error fetching data from API: {ex.Message}");
            }
        }

        public class OrchidViewModel
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

        public class PagedResponse<T>
        {
            public List<T> Items { get; set; } = new List<T>();
            public int TotalCount { get; set; }
            public int PageCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }
    }
}