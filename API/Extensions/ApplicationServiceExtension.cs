using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

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
        services.AddSwaggerGen();

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