using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StockPile.Infrastructure.Persistence;

namespace StockPile.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "StockPile";

    /// <summary>
    /// Registers infrastructure implementations of the Application-layer abstractions.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        // Registration is lazy: a connection is only opened when a service resolves the DbContext,
        // so the API and /health can start without a database during scaffolding and smoke tests.
        services.AddDbContext<StockPileDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
