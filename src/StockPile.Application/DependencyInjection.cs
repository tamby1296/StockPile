using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace StockPile.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers application services and every FluentValidation validator in this assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(ApplicationAssemblyMarker), includeInternalTypes: true);
        return services;
    }
}
