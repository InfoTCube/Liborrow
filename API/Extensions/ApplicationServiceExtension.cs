using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

namespace API.Extensions;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration config)
    {
        // SQLite configuration
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your valid token."
            });
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddBibliotekaNarodowaService(this IServiceCollection services)
    {
        services.AddHttpClient<IBibliotekaNarodowaBooksService, BibliotekaNarodowaBooksService>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Liborrow/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}