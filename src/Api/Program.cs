using Api.Endpoints;
using Api.Extensions;
using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true);
builder.Services.AddApiServices(builder.Configuration);
var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRateLimiter();
app.MapInvoiceEndpoints();
app.Run();
