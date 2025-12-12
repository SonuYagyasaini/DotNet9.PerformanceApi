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

        if (!string.IsNullOrWhiteSpace(sbConn))
        {
            services.AddSingleton(new ServiceBusClient(sbConn));
        }
        else if (!string.IsNullOrWhiteSpace(sbNamespace))
        {
            services.AddSingleton(sp =>
                new ServiceBusClient(sbNamespace, new DefaultAzureCredential()));
        }
        else
        {
            throw new InvalidOperationException(
                "ServiceBus is not configured. Set ServiceBus:Conn or ServiceBus:Namespace for Managed Identity.");
        }

        services.AddScoped<IInvoiceService, InvoiceService>();

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

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

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
