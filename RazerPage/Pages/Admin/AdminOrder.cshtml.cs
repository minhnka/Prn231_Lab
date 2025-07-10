using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace RazerPage.Pages.Admin
{
    [Authorize(Roles = "1")]
    public class AdminOrderModel : PageModel
    {
        private readonly ILogger<AdminOrderModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminOrderModel(ILogger<AdminOrderModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public List<OrderViewModel> Orders { get; private set; } = new();
        public OrderViewModel? SelectedOrder { get; private set; }
        public string ErrorMessage { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? OrderId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (OrderId.HasValue)
            {
                // Load a specific order's details
                await LoadOrderDetails(OrderId.Value);
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = User.FindFirst("AccessToken")?.Value;

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Construct API URL with optional filters
                    var apiUrl = "http://localhost:5164/api/Order";
                    _logger.LogInformation("Attempting to fetch orders from: {ApiUrl}", apiUrl);

                    var queryParams = new List<string>();

                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        queryParams.Add($"search={Uri.EscapeDataString(SearchTerm)}");
                    }

                    if (!string.IsNullOrEmpty(StatusFilter))
                    {
                        queryParams.Add($"status={Uri.EscapeDataString(StatusFilter)}");
                    }

                    if (queryParams.Count > 0)
                    {
                        apiUrl += "?" + string.Join("&", queryParams);
                    }

                    var response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("API Response: {Content}", content);

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            AllowTrailingCommas = true
                        };

                        try
                        {
                            // Parse the raw JSON to handle the specific nested structure
                            using (JsonDocument document = JsonDocument.Parse(content))
                            {
                                // Check if the root element has a $values property
                                if (document.RootElement.TryGetProperty("$values", out JsonElement valuesArray))
                                {
                                    foreach (JsonElement orderElement in valuesArray.EnumerateArray())
                                    {
                                        // Create view model for the order
                                        var viewModel = new OrderViewModel();

                                        // Extract order properties
                                        if (orderElement.TryGetProperty("id", out JsonElement idElement))
                                            viewModel.Id = idElement.GetInt32();

                                        if (orderElement.TryGetProperty("accountId", out JsonElement accountIdElement) &&
                                            accountIdElement.ValueKind != JsonValueKind.Null)
                                            viewModel.AccountId = accountIdElement.GetInt32();

                                        if (orderElement.TryGetProperty("orderDate", out JsonElement dateElement) &&
                                            dateElement.ValueKind != JsonValueKind.Null)
                                        {
                                            if (DateOnly.TryParse(dateElement.GetString(), out DateOnly date))
                                                viewModel.OrderDate = date;
                                        }

                                        if (orderElement.TryGetProperty("orderStatus", out JsonElement statusElement) &&
                                            statusElement.ValueKind != JsonValueKind.Null)
                                            viewModel.OrderStatus = statusElement.GetString() ?? "Pending";
                                        else
                                            viewModel.OrderStatus = "Pending";

                                        if (orderElement.TryGetProperty("totalAmount", out JsonElement amountElement) &&
                                            amountElement.ValueKind != JsonValueKind.Null)
                                            viewModel.TotalAmount = amountElement.GetDecimal();

                                        // Get customer email and name if we have an account ID
                                        if (viewModel.AccountId.HasValue)
                                        {
                                            try
                                            {
                                                var accountResponse = await client.GetAsync($"http://localhost:5164/api/Account/{viewModel.AccountId}");
                                                if (accountResponse.IsSuccessStatusCode)
                                                {
                                                    var accountContent = await accountResponse.Content.ReadAsStringAsync();
                                                    using (JsonDocument accountDoc = JsonDocument.Parse(accountContent))
                                                    {
                                                        if (accountDoc.RootElement.TryGetProperty("email", out JsonElement emailElement) &&
                                                            emailElement.ValueKind != JsonValueKind.Null)
                                                            viewModel.CustomerEmail = emailElement.GetString();
                                                        else
                                                            viewModel.CustomerEmail = $"Account #{viewModel.AccountId}";

                                                        if (accountDoc.RootElement.TryGetProperty("accountName", out JsonElement accountNameElement) &&
                                                            accountNameElement.ValueKind != JsonValueKind.Null)
                                                            viewModel.CustomerName = accountNameElement.GetString();
                                                        else if (accountDoc.RootElement.TryGetProperty("name", out JsonElement nameElement) &&
                                                            nameElement.ValueKind != JsonValueKind.Null)
                                                            viewModel.CustomerName = nameElement.GetString();
                                                        else if (accountDoc.RootElement.TryGetProperty("fullName", out JsonElement fullNameElement) &&
                                                            fullNameElement.ValueKind != JsonValueKind.Null)
                                                            viewModel.CustomerName = fullNameElement.GetString();
                                                        else
                                                            viewModel.CustomerName = viewModel.CustomerEmail;
                                                    }
                                                }
                                                else
                                                {
                                                    viewModel.CustomerEmail = $"Account #{viewModel.AccountId}";
                                                    viewModel.CustomerName = $"Account #{viewModel.AccountId}";
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogWarning(ex, "Failed to get account details");
                                                viewModel.CustomerEmail = $"Account #{viewModel.AccountId}";
                                                viewModel.CustomerName = $"Account #{viewModel.AccountId}";
                                            }
                                        }
                                        else
                                        {
                                            viewModel.CustomerEmail = "N/A";
                                            viewModel.CustomerName = "N/A";
                                        }

                                        // Process order details if available
                                        if (orderElement.TryGetProperty("orderDetails", out JsonElement orderDetailsElement) &&
                                            orderDetailsElement.ValueKind != JsonValueKind.Null &&
                                            orderDetailsElement.TryGetProperty("$values", out JsonElement detailsArray))
                                        {
                                            foreach (JsonElement detailElement in detailsArray.EnumerateArray())
                                            {
                                                var itemViewModel = new OrderItemViewModel();

                                                // Extract item properties
                                                if (detailElement.TryGetProperty("id", out JsonElement detailIdElement))
                                                    itemViewModel.Id = detailIdElement.GetInt32();

                                                if (detailElement.TryGetProperty("orchidId", out JsonElement orchidIdElement) &&
                                                    orchidIdElement.ValueKind != JsonValueKind.Null)
                                                    itemViewModel.OrchidId = orchidIdElement.GetInt32();

                                                if (detailElement.TryGetProperty("price", out JsonElement priceElement) &&
                                                    priceElement.ValueKind != JsonValueKind.Null)
                                                    itemViewModel.Price = priceElement.GetDecimal();

                                                if (detailElement.TryGetProperty("quantity", out JsonElement quantityElement) &&
                                                    quantityElement.ValueKind != JsonValueKind.Null)
                                                    itemViewModel.Quantity = quantityElement.GetInt32();

                                                // Get product name if we have an orchid ID
                                                if (itemViewModel.OrchidId.HasValue)
                                                {
                                                    try
                                                    {
                                                        var orchidResponse = await client.GetAsync($"http://localhost:5164/api/Orchid/{itemViewModel.OrchidId}");
                                                        if (orchidResponse.IsSuccessStatusCode)
                                                        {
                                                            var orchidContent = await orchidResponse.Content.ReadAsStringAsync();
                                                            using (JsonDocument orchidDoc = JsonDocument.Parse(orchidContent))
                                                            {
                                                                if (orchidDoc.RootElement.TryGetProperty("orchidName", out JsonElement nameElement) &&
                                                                    nameElement.ValueKind != JsonValueKind.Null)
                                                                    itemViewModel.ProductName = nameElement.GetString();
                                                                else
                                                                    itemViewModel.ProductName = $"Product #{itemViewModel.OrchidId}";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            itemViewModel.ProductName = $"Product #{itemViewModel.OrchidId}";
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        itemViewModel.ProductName = $"Product #{itemViewModel.OrchidId}";
                                                    }
                                                }
                                                else
                                                {
                                                    itemViewModel.ProductName = "Unknown Product";
                                                }

                                                viewModel.Items.Add(itemViewModel);
                                            }
                                        }

                                        Orders.Add(viewModel);
                                    }

                                    _logger.LogInformation("Successfully mapped {Count} orders to view models", Orders.Count);
                                }
                                else
                                {
                                    _logger.LogWarning("No $values array found in the response");
                                    ErrorMessage = "Invalid response format from the server.";
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize order data: {Message}", ex.Message);
                            ErrorMessage = "Failed to process order data from the server.";
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Failed to retrieve orders: {StatusCode}, Response: {Content}",
                            response.StatusCode, errorContent);
                        ErrorMessage = $"Failed to retrieve orders. Status code: {response.StatusCode}";
                    }
                }
                else
                {
                    ErrorMessage = "Authorization token not found. Please log in again.";
                    _logger.LogWarning("Authorization token not found for user {User}", User.Identity?.Name);
                    return RedirectToPage("/Login");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error: {Message}", ex.Message);
                ErrorMessage = $"Could not connect to the order service. {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders: {Message}", ex.Message);
                ErrorMessage = "An error occurred while retrieving orders.";
            }

            return Page();
        }

        // New method to load a specific order's details
        private async Task LoadOrderDetails(int orderId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = User.FindFirst("AccessToken")?.Value;

                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "Authorization token not found. Please log in again.";
                    return;
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var apiUrl = $"http://localhost:5164/api/Order/{orderId}";
                _logger.LogInformation("Fetching order details from: {ApiUrl}", apiUrl);

                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Order details response: {Content}", content);

                    try
                    {
                        using (JsonDocument document = JsonDocument.Parse(content))
                        {
                            var orderElement = document.RootElement;

                            // Create view model for the order
                            var viewModel = new OrderViewModel();

                            // Extract order properties
                            if (orderElement.TryGetProperty("id", out JsonElement idElement))
                                viewModel.Id = idElement.GetInt32();

                            if (orderElement.TryGetProperty("accountId", out JsonElement accountIdElement) &&
                                accountIdElement.ValueKind != JsonValueKind.Null)
                                viewModel.AccountId = accountIdElement.GetInt32();

                            if (orderElement.TryGetProperty("orderDate", out JsonElement dateElement) &&
                                dateElement.ValueKind != JsonValueKind.Null)
                            {
                                if (DateOnly.TryParse(dateElement.GetString(), out DateOnly date))
                                    viewModel.OrderDate = date;
                            }

                            if (orderElement.TryGetProperty("orderStatus", out JsonElement statusElement) &&
                                statusElement.ValueKind != JsonValueKind.Null)
                                viewModel.OrderStatus = statusElement.GetString() ?? "Pending";
                            else
                                viewModel.OrderStatus = "Pending";

                            if (orderElement.TryGetProperty("totalAmount", out JsonElement amountElement) &&
                                amountElement.ValueKind != JsonValueKind.Null)
                                viewModel.TotalAmount = amountElement.GetDecimal();

                            // Get customer email and name if we have an account ID
                            if (viewModel.AccountId.HasValue)
                            {
                                try
                                {
                                    var accountResponse = await client.GetAsync($"http://localhost:5164/api/Account/{viewModel.AccountId}");
                                    if (accountResponse.IsSuccessStatusCode)
                                    {
                                        var accountContent = await accountResponse.Content.ReadAsStringAsync();
                                        using (JsonDocument accountDoc = JsonDocument.Parse(accountContent))
                                        {
                                            if (accountDoc.RootElement.TryGetProperty("email", out JsonElement emailElement) &&
                                                emailElement.ValueKind != JsonValueKind.Null)
                                                viewModel.CustomerEmail = emailElement.GetString();
                                            else
                                                viewModel.CustomerEmail = $"Account #{viewModel.AccountId}";

                                            if (accountDoc.RootElement.TryGetProperty("accountName", out JsonElement accountNameElement) &&
                                                accountNameElement.ValueKind != JsonValueKind.Null)
                                                viewModel.CustomerName = accountNameElement.GetString();
                                            else if (accountDoc.RootElement.TryGetProperty("name", out JsonElement nameElement) &&
                                                nameElement.ValueKind != JsonValueKind.Null)
                                                viewModel.CustomerName = nameElement.GetString();
                                            else if (accountDoc.RootElement.TryGetProperty("fullName", out JsonElement fullNameElement) &&
                                                fullNameElement.ValueKind != JsonValueKind.Null)
                                                viewModel.CustomerName = fullNameElement.GetString();
                                            else
                                                viewModel.CustomerName = viewModel.CustomerEmail;
                                        }
                                    }
                                    else
                                    {
                                        viewModel.CustomerEmail = $"Account #{viewModel.AccountId}";
                                        viewModel.CustomerName = $"Account #{viewModel.AccountId}";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to get account details");
                                    viewModel.CustomerEmail = $"Account #{viewModel.AccountId}";
                                    viewModel.CustomerName = $"Account #{viewModel.AccountId}";
                                }
                            }
                            else
                            {
                                viewModel.CustomerEmail = "N/A";
                                viewModel.CustomerName = "N/A";
                            }

                            // Process order details if available
                            if (orderElement.TryGetProperty("orderDetails", out JsonElement orderDetailsElement) &&
                                orderDetailsElement.ValueKind != JsonValueKind.Null)
                            {
                                JsonElement detailsArray;

                                // Check if orderDetails has a $values property or is an array directly
                                if (orderDetailsElement.TryGetProperty("$values", out JsonElement valuesElement))
                                {
                                    detailsArray = valuesElement;
                                }
                                else if (orderDetailsElement.ValueKind == JsonValueKind.Array)
                                {
                                    detailsArray = orderDetailsElement;
                                }
                                else
                                {
                                    _logger.LogWarning("Order details not in expected format");
                                    detailsArray = default;
                                }

                                if (detailsArray.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (JsonElement detailElement in detailsArray.EnumerateArray())
                                    {
                                        var itemViewModel = new OrderItemViewModel();

                                        // Extract item properties
                                        if (detailElement.TryGetProperty("id", out JsonElement detailIdElement))
                                            itemViewModel.Id = detailIdElement.GetInt32();

                                        if (detailElement.TryGetProperty("orchidId", out JsonElement orchidIdElement) &&
                                            orchidIdElement.ValueKind != JsonValueKind.Null)
                                            itemViewModel.OrchidId = orchidIdElement.GetInt32();

                                        if (detailElement.TryGetProperty("price", out JsonElement priceElement) &&
                                            priceElement.ValueKind != JsonValueKind.Null)
                                            itemViewModel.Price = priceElement.GetDecimal();

                                        if (detailElement.TryGetProperty("quantity", out JsonElement quantityElement) &&
                                            quantityElement.ValueKind != JsonValueKind.Null)
                                            itemViewModel.Quantity = quantityElement.GetInt32();

                                        // Get product name if we have an orchid ID
                                        if (itemViewModel.OrchidId.HasValue)
                                        {
                                            try
                                            {
                                                var orchidResponse = await client.GetAsync($"http://localhost:5164/api/Orchid/{itemViewModel.OrchidId}");
                                                if (orchidResponse.IsSuccessStatusCode)
                                                {
                                                    var orchidContent = await orchidResponse.Content.ReadAsStringAsync();
                                                    using (JsonDocument orchidDoc = JsonDocument.Parse(orchidContent))
                                                    {
                                                        if (orchidDoc.RootElement.TryGetProperty("orchidName", out JsonElement nameElement) &&
                                                            nameElement.ValueKind != JsonValueKind.Null)
                                                            itemViewModel.ProductName = nameElement.GetString();
                                                        else
                                                            itemViewModel.ProductName = $"Product #{itemViewModel.OrchidId}";
                                                    }
                                                }
                                                else
                                                {
                                                    itemViewModel.ProductName = $"Product #{itemViewModel.OrchidId}";
                                                }
                                            }
                                            catch
                                            {
                                                itemViewModel.ProductName = $"Product #{itemViewModel.OrchidId}";
                                            }
                                        }
                                        else
                                        {
                                            itemViewModel.ProductName = "Unknown Product";
                                        }

                                        viewModel.Items.Add(itemViewModel);
                                    }
                                }
                            }

                            SelectedOrder = viewModel;
                            _logger.LogInformation("Successfully loaded order details for order {OrderId}", orderId);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize order detail data: {Message}", ex.Message);
                        ErrorMessage = "Failed to process order details from the server.";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to retrieve order details: {StatusCode}, Response: {Content}",
                        response.StatusCode, errorContent);
                    ErrorMessage = $"Failed to retrieve order details. Status code: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details: {Message}", ex.Message);
                ErrorMessage = "An error occurred while retrieving order details.";
            }
        }

        // View models for displaying order data
        public class OrderViewModel
        {
            public int Id { get; set; }
            public int? AccountId { get; set; }
            public string CustomerName { get; set; } = "";
            public string CustomerEmail { get; set; } = "N/A";
            public DateOnly? OrderDate { get; set; }
            public string OrderStatus { get; set; } = "Pending";
            public decimal TotalAmount { get; set; }
            public List<OrderItemViewModel> Items { get; set; } = new();
        }

        public class OrderItemViewModel
        {
            public int Id { get; set; }
            public int? OrchidId { get; set; }
            public string ProductName { get; set; } = "Unknown";
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }
    }
}