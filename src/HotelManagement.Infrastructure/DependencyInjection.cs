using System.Text;

using HotelManagement.Application.Common.Interfaces.Authentication;
using HotelManagement.Application.Common.Interfaces.Persistence;
using HotelManagement.Application.Common.Interfaces.Services;
using HotelManagement.Infrastructure.Authentication;
using HotelManagement.Infrastructure.Persistence;
using HotelManagement.Infrastructure.Persistence.Interceptors;
using HotelManagement.Infrastructure.Persistence.Repositories;
using HotelManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HotelManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services
            .AddAuth(configuration)
            .AddPersistance();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPaymentService, PaymentService>();

        return services;
    }

    public static IServiceCollection AddPersistance(
        this IServiceCollection services)
    {
        services.AddDbContext<HotelManagementDbContext>(options =>
            options.UseSqlServer());

        services.AddScoped<PublishDomainEventsInterceptor>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        return services;
    }

    public static IServiceCollection AddAuth(
            this IServiceCollection services,
            ConfigurationManager configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);

        services.AddSingleton(Options.Create(jwtSettings));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            });

        return services;
    }
}
