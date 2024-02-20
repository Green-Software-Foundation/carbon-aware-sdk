using CarbonAware.Configuration;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace CarbonAware.Extensions;
internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataSourceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var noEmissionsDataSource = false;
        var noForecastDataSource = false;

        try
        {
            services.AddDataSource<IEmissionsDataSource>(configuration);
        }
        catch (Exception)
        {
            services.TryAddSingleton<IEmissionsDataSource, NullEmissionsDataSource>();
            noEmissionsDataSource = true;
        }

        try
        {
            services.AddDataSource<IForecastDataSource>(configuration);
        }
        catch (Exception)
        {
            services.TryAddSingleton<IForecastDataSource, NullForecastDataSource>();
            noForecastDataSource = true;
        }

        if (noEmissionsDataSource && noForecastDataSource)
        {
            throw new ConfigurationException("No data sources are configured");
        }

        return services;
    }

    public static IServiceCollection AddDataSource<T>(this IServiceCollection services, IConfiguration configuration)
        where T : IDataSource
    {
        // Get the config
        var dataSources = configuration.DataSources();
        var configurationType = dataSources.ConfigurationType<T>();
        
        // Load the assembly for the configured IDataSource interface, T. 
        Assembly assembly;
        try
        {
            assembly = Assembly.Load(configurationType);
        }
        catch (FileNotFoundException)
        {
            try
            {
                assembly = Assembly.Load($"CarbonAware.DataSources.{configurationType}");
            }
            catch (Exception e)
            {
                throw new ConfigurationException($"Could not load assembly for data source '{configurationType}'", e);
            }
        }


        // Get the classes that implement the interface, T.
        // Pick the first, because we only expect one per interface.
        Type dataSourceType = assembly.GetTypes()
            .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .First() ?? throw new ConfigurationException("No data sources are configured");

        // Call static configuration method on the data source to allow it
        // to configure itself and its dependencies.
        MethodInfo configureMethod = dataSourceType.GetMethod(
            "ConfigureDI",
            BindingFlags.Static
            | BindingFlags.Public)
            ?.MakeGenericMethod(typeof(T)) ?? throw new ConfigurationException("No data method 'ConfigureDI' is configured");
    
        configureMethod.Invoke(null, new Object[2] {services, dataSources});

        return services;
    }
}
