var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Login";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();



app.UseRouting();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Redirect root URL to Login page
app.MapGet("/", () => Results.Redirect("/Login"));

app.MapRazorPages();

app.Run();
