using System.Text;
using Amazon.SimpleNotificationService;
using CPF.Application.Interfaces;
using CPF.Infrastructure.Data;
using CPF.Infrastructure.Services;
using CPF.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace CPF.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // --- Database ---
        var connectionString = configuration.GetConnectionString("PostgreSQL");

        if (environment.IsDevelopment() && string.IsNullOrEmpty(connectionString))
        {
            // In-memory DB for local dev/testing when no PostgreSQL is available
            services.AddDbContext<CpfDbContext>(options =>
                options.UseInMemoryDatabase("CpfDev"));
        }
        else
        {
            services.AddDbContext<CpfDbContext>(options =>
                options.UseNpgsql(connectionString,
                    npgsql => npgsql.EnableRetryOnFailure(3)));
        }

        // --- Settings ---
        var jwtSettings = configuration.GetSection("Jwt");
        services.Configure<JwtSettings>(jwtSettings);
        services.Configure<YocoSettings>(configuration.GetSection("Yoco"));

        // --- JWT Authentication ---
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        services.AddAuthorization();

        // --- AWS Services ---
        if (environment.IsDevelopment())
        {
            // Stub notification service for local dev (no real SNS)
            services.AddScoped<INotificationService, StubNotificationService>();
        }
        else
        {
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            services.AddScoped<INotificationService, SnsNotificationService>();
        }

        // --- Application Services ---
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPanicAlertService, PanicAlertService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IPostService, PostService>();

        // --- Yoco HttpClient ---
        services.AddHttpClient<IPaymentService, YocoPaymentService>();

        return services;
    }
}
