using Api.Endpoints;
using Api.Extensions;
using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true);

// Register application services (must contain AddAuthentication + AddAuthorization)
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Global exception handler middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable Swagger before auth (so swagger UI loads without token)
// Serve Swagger JSON at the standard path and ensure the UI points to it first.
app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
    // primary (correct) endpoint
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1");
    // legacy/alternate endpoint to tolerate cached UI or different route templates
    options.SwaggerEndpoint("/v1/swagger.json", "Api v1 (alternate)");
});

// Authentication & Authorization middlewares
app.UseAuthentication();
app.UseAuthorization();

// Rate Limiting
app.UseRateLimiter();

// Map Endpoints
app.MapInvoiceEndpoints();

app.Run();
