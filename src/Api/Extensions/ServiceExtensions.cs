using Application.Interfaces;
using Application.Services;
using Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Azure.Identity;
using Api.Middleware;
using Microsoft.OpenApi.Models;

namespace Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("Db")));

        services.AddMemoryCache();

        var sbConn = config.GetValue<string>("ServiceBus:Conn");
        var sbNamespace = config["ServiceBus:Namespace"]; // e.g. "mynamespace.servicebus.windows.net"
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        if (!string.IsNullOrWhiteSpace(sbConn))
        {
            services.AddSingleton(new ServiceBusClient(sbConn));
        }
        else if (!string.IsNullOrWhiteSpace(sbNamespace))
        {
            services.AddSingleton(sp =>
                new ServiceBusClient(sbNamespace, new DefaultAzureCredential()));
        }
        else if (!isDevelopment)
        {
            throw new InvalidOperationException(
                "ServiceBus is not configured. Set ServiceBus:Conn or ServiceBus:Namespace.");
        }
        else
        {
            // no Service Bus for local env
            services.AddSingleton<ServiceBusClient?>(_ => null);
        }

        // Register application services
        services.AddScoped<IInvoiceService, InvoiceService>();

        // Register middleware implemented as IMiddleware so it can be resolved by UseMiddleware<T>()
        services.AddScoped<GlobalExceptionMiddleware>();

        var key = config["Jwt:Key"];
        if (!string.IsNullOrEmpty(key))
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                    };
                });
        }

        // Ensure authorization services are registered so app.UseAuthorization() succeeds
        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        // Configure Swagger generation with a v1 document so /swagger/v1/swagger.json is available
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
        });

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", o =>
            {
                o.PermitLimit = 200;
                o.Window = TimeSpan.FromSeconds(1);
            });
        });

        return services;
    }
}
